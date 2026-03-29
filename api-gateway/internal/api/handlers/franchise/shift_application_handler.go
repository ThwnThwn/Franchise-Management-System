package franchise

import (
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
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
// @Summary      Apply for a shift
// @Description  Employee applies for a specific shift. Creates a new ShiftApplication record. UserId is extracted from the authentication token.
// @Tags         Shifts: Applications
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path  string true "Shift ID"
// @Param        body  body  shift.ApplyForShiftRequest true "Application notes and details"
// @Success      201   {object} map[string]string "{"message": "Gửi đơn đăng ký nhận ca thành công"}"
// @Failure      400   {object} object "Validation Error (Bad Request)"
// @Failure      401   {object} object "Unauthorized"
// @Failure      500   {object} object "Internal Server Error"
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

	ctx := c.Request.Context()
	if err := h.appGW.Apply(ctx, shiftId, userId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusCreated, gin.H{"message": "Gửi đơn đăng ký nhận ca thành công"})
	return nil
}

// GetByShiftId
// @Summary      Get applications by shift ID
// @Description  Manager views the list of applicants for a specific shift, along with their Smart Scores, to make approval decisions.
// @Tags         Shifts: Applications
// @Security     BearerAuth
// @Produce      json
// @Param        id    path  string true "Shift ID"
// @Success      200   {array}  shift.ShiftApplicationItem "List of shift applications"
// @Failure      400   {object} object "Invalid Shift ID (Bad Request)"
// @Failure      401   {object} object "Unauthorized"
// @Failure      500   {object} object "Internal Server Error"
// @Router       /shifts/{id}/applications [get]
func (h *ShiftApplicationHandler) GetByShiftId(c *gin.Context) error {
	shiftId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	data, err := h.appGW.GetByShiftId(ctx, shiftId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, data)
	return nil
}

// Review
// @Summary      Review shift application
// @Description  Manager reviews an application and updates its status (Approved/Rejected). The application ID is provided in the path.
// @Tags         Shifts: Applications
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path  string true "Application ID"
// @Param        body  body  shift.ReviewApplicationRequest true "Review decision and status"
// @Success      200   {object} map[string]string "{"message": "Đã cập nhật trạng thái đơn đăng ký"}"
// @Failure      400   {object} object "Validation Error (Bad Request)"
// @Failure      401   {object} object "Unauthorized"
// @Failure      500   {object} object "Internal Server Error"
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

	ctx := c.Request.Context()
	if err := h.appGW.Review(ctx, appId, managerId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã cập nhật trạng thái đơn đăng ký"})
	return nil
}
