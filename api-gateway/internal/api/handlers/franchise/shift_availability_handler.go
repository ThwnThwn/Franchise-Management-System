package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/validation"
	"fmt"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftAvailabilityHandler struct {
	shiftGW franchise.IShiftService
}

func NewShiftAvailabilityHandler(shiftGW franchise.IShiftService) *ShiftAvailabilityHandler {
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
func (h *ShiftAvailabilityHandler) GetMyHistory(c *gin.Context) {
	userID, err := contextutils.GetUserID(c)
	if err != nil {
		c.Error(err)
		return
	}

	var params shift.GetAvailabilityParams
	if err := c.ShouldBindQuery(&params); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(userID, params)
	// res, err := h.shiftGW.GetAvailability(c.Request.Context(), &userID, params)
	res := gin.H{}
	c.JSON(http.StatusOK, res)
}

// Create
// @Summary      Đăng ký lịch rảnh/bận mới
// @Description  Gửi đơn đăng ký thời gian rảnh hoặc bận để quản lý sắp ca.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Param        body body shift.CreateShiftAvailabilityRequest true "Dữ liệu đăng ký"
// @Router       /shift-availabilities [post]
func (h *ShiftAvailabilityHandler) Create(c *gin.Context) {
	userID, err := contextutils.GetUserID(c)
	if err != nil {
		c.Error(err)
		return
	}

	var req shift.CreateShiftAvailabilityRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(userID, req)
	// err = h.shiftGW.CreateAvailability(c.Request.Context(), userID, req)

	c.JSON(http.StatusCreated, gin.H{})
}

// Update
// @Summary      Cập nhật lịch rảnh/bận
// @Description  Chỉnh sửa đơn đăng ký lịch rảnh/bận đã gửi dựa trên ID.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Param        id   path string true "ID lịch đăng ký"
// @Param        body body shift.UpdateShiftAvailabilityRequest true "Dữ liệu cập nhật"
// @Router       /shift-availabilities/{id} [patch]
func (h *ShiftAvailabilityHandler) Update(c *gin.Context) {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req shift.UpdateShiftAvailabilityRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(id, req)
	// err = h.shiftGW.UpdateAvailability(c.Request.Context(), id, req)

	c.JSON(http.StatusOK, gin.H{})
}

// Delete
// @Summary      Hủy/Xóa lịch rảnh/bận
// @Description  Xóa bỏ đơn đăng ký lịch rảnh/bận đã gửi.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Param        id path string true "ID lịch đăng ký"
// @Router       /shift-availabilities/{id} [delete]
func (h *ShiftAvailabilityHandler) Delete(c *gin.Context) {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	fmt.Println(id)
	// err = h.shiftGW.DeleteAvailability(c.Request.Context(), id)

	c.JSON(http.StatusOK, gin.H{})
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
func (h *ShiftAvailabilityHandler) GetAll(c *gin.Context) {
	var params shift.GetAvailabilityParams
	if err := c.ShouldBindQuery(&params); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(params)
	// res, err := h.shiftGW.GetAllAvailability(c.Request.Context(), params)
	res := gin.H{}

	c.JSON(http.StatusOK, res)
}
