package franchise

import (
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftApplicationHandler struct {
	appGW franchise.IShiftApplicationService
}

func NewShiftApplicationHandler(appGW franchise.IShiftApplicationService) *ShiftApplicationHandler {
	return &ShiftApplicationHandler{appGW: appGW}
}

// Apply
// @Summary      Nhân viên đăng ký nhận ca (Tạo mới một đơn đăng ký)
// @Description  Tạo một bản ghi ShiftApplication cho ca làm việc cụ thể. UserId lấy từ Token.
// @Tags         Shifts: Applications
// @Accept       json
// @Produce      json
// @Param        id path string true "Shift ID"
// @Param        body body shift.ApplyForShiftRequest true "Ghi chú đăng ký"
// @Router       /shifts/{id}/applications [post]
func (h *ShiftApplicationHandler) Apply(c *gin.Context) error {
	shiftId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return validation.TranslateValidationError(err)
	}

	var req shift.ApplyForShiftRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	if err := h.appGW.Apply(ctx, shiftId, userId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusCreated, gin.H{"message": "Gửi đơn đăng ký nhận ca thành công"})
	return nil
}

// GetByShiftId
// @Summary      Lấy danh sách các đơn đăng ký của một ca làm việc
// @Description  Manager xem danh sách ứng viên kèm điểm Smart Score để đưa ra quyết định duyệt.
// @Tags         Shifts: Applications
// @Produce      json
// @Param        id path string true "Shift ID"
// @Router       /shifts/{id}/applications [get]
func (h *ShiftApplicationHandler) GetByShiftId(c *gin.Context) error {
	shiftId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	data, err := h.appGW.GetByShiftId(ctx, shiftId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, data)
	return nil
}

// Review
// @Summary      Duyệt hoặc từ chối đơn đăng ký nhận ca
// @Description  Manager cập nhật trạng thái đơn (Approved/Rejected). ID đơn lấy từ Path.
// @Tags         Shifts: Applications
// @Accept       json
// @Produce      json
// @Param        id path string true "Application ID"
// @Param        body body shift.ReviewApplicationRequest true "Kết quả duyệt đơn"
// @Router       /shift-applications/{id}/review [patch]
func (h *ShiftApplicationHandler) Review(c *gin.Context) error {
	appId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return validation.TranslateValidationError(err)
	}

	var req shift.ReviewApplicationRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	managerId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	if err := h.appGW.Review(ctx, appId, managerId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã cập nhật trạng thái đơn đăng ký"})
	return nil
}
