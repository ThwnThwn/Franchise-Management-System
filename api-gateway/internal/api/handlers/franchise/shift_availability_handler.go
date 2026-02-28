package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/stringutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftAvailabilityHandler struct {
	shiftGW franchise.IShiftAvailabilityService
}

func NewShiftAvailabilityHandler(shiftGW franchise.IShiftAvailabilityService) *ShiftAvailabilityHandler {
	return &ShiftAvailabilityHandler{shiftGW: shiftGW}
}

// ===============================
// STAFF SIDE (Của tôi)
// ===============================

// GetMyHistory
// @Summary      Lấy lịch sử đăng ký rảnh/bận của tôi
// @Description  Truy xuất danh sách lịch rảnh/bận đã đăng ký của nhân viên hiện tại kèm phân trang.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Param        query query shift.GetAvailabilityParams false "Bộ lọc tìm kiếm"
// @Router       /me/shift-availabilities [get]
func (h *ShiftAvailabilityHandler) GetMyHistory(c *gin.Context) error {
	userID, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	var params shift.GetAvailabilityParams
	if err := c.ShouldBindQuery(&params); err != nil {
		return validation.TranslateValidationError(err)
	}

	params.UserID = stringutils.ToPtr(userID.String())
	res, err := h.shiftGW.GetAll(c.Request.Context(), &params)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Create
// @Summary      Đăng ký lịch rảnh/bận mới
// @Description  Gửi đơn đăng ký thời gian rảnh hoặc bận để quản lý sắp ca.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Param        body body shift.CreateShiftAvailabilityRequest true "Dữ liệu đăng ký"
// @Router       /shift-availabilities [post]
func (h *ShiftAvailabilityHandler) Create(c *gin.Context) error {
	userID, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	var req shift.CreateShiftAvailabilityRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	err = h.shiftGW.Create(ctx, userID, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusCreated, gin.H{})
	return nil
}

// Update
// @Summary      Cập nhật lịch rảnh/bận
// @Description  Chỉnh sửa đơn đăng ký lịch rảnh/bận đã gửi dựa trên ID.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Param        id   path string true "ID lịch đăng ký"
// @Param        body body shift.UpdateShiftAvailabilityRequest true "Dữ liệu cập nhật"
// @Router       /shift-availabilities/{id} [patch]
func (h *ShiftAvailabilityHandler) Update(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req shift.UpdateShiftAvailabilityRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	err = h.shiftGW.Update(ctx, id, userId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// Delete
// @Summary      Hủy/Xóa lịch rảnh/bận
// @Description  Xóa bỏ đơn đăng ký lịch rảnh/bận đã gửi.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Param        id path string true "ID lịch đăng ký"
// @Router       /shift-availabilities/{id} [delete]
func (h *ShiftAvailabilityHandler) Delete(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	err = h.shiftGW.Delete(ctx, id, userId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// ===============================
// MANAGER SIDE (Quản lý)
// ===============================

// GetAll
// @Summary      Quản lý toàn bộ lịch rảnh/bận
// @Description  Quản lý xem toàn bộ danh sách lịch rảnh/bận của nhân viên để sắp ca làm việc.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Param        query query shift.GetAvailabilityParams false "Bộ lọc tìm kiếm (Store, User, Date...)"
// @Router       /shift-availabilities [get]
func (h *ShiftAvailabilityHandler) GetAll(c *gin.Context) error {
	var params shift.GetAvailabilityParams
	if err := c.ShouldBindQuery(&params); err != nil {
		return validation.TranslateValidationError(err)
	}

	res, err := h.shiftGW.GetAll(c.Request.Context(), &params)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}
