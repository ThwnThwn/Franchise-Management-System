package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftSwapHandler struct {
	swapGW franchise.IShiftSwapService
}

func NewShiftSwapHandler(swapGW franchise.IShiftSwapService) *ShiftSwapHandler {
	return &ShiftSwapHandler{swapGW: swapGW}
}

// RequestSwap
// @Summary      Request a shift swap
// @Description  Employee A selects one of their assignments and requests a swap with Employee B.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path   string true "Assignment ID (Sender's shift)"
// @Param        body body   shift.RequestSwapRequest true "Swap request details"
// @Success      201  {object} map[string]interface{} "{"message": "Đã gửi yêu cầu đổi ca"}"
// @Failure      400  {object} object "Validation Error (Bad Request)"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts/{id}/swaps [post]
func (h *ShiftSwapHandler) RequestSwap(c *gin.Context) error {
	assignmentId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req shift.RequestSwapRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx := c.Request.Context()
	if err := h.swapGW.RequestSwap(ctx, assignmentId, userId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusCreated, gin.H{"message": "Đã gửi yêu cầu đổi ca"})
	return nil
}

// RespondSwap
// @Summary      Respond to a shift swap request
// @Description  Employee B receives the swap request and chooses to Accept or Reject it.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path   string true "Swap Request ID"
// @Param        body body   shift.RespondSwapRequest true "Respondent's reply data"
// @Success      200  {object} map[string]interface{} "{"message": "Đã ghi nhận phản hồi đổi ca"}"
// @Failure      400  {object} object "Validation Error or Invalid ID"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shift-swaps/{id}/responses [patch]
func (h *ShiftSwapHandler) RespondSwap(c *gin.Context) error {
	swapId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req shift.RespondSwapRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx := c.Request.Context()
	if err := h.swapGW.RespondSwap(ctx, swapId, userId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã ghi nhận phản hồi đổi ca"})
	return nil
}

// ApproveSwap
// @Summary      Approve a shift swap (Manager)
// @Description  After both employees agree to the swap, the Manager gives final approval to officially update the schedule.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path   string true "Swap Request ID"
// @Param        body body   shift.ApproveSwapRequest true "Manager's approval decision"
// @Success      200  {object} map[string]interface{} "{"message": "Đã hoàn tất duyệt đổi ca"}"
// @Failure      400  {object} object "Validation Error or Invalid ID"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shift-swaps/{id}/approvals [patch]
func (h *ShiftSwapHandler) ApproveSwap(c *gin.Context) error {
	swapId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req shift.ApproveSwapRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	managerId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx := c.Request.Context()
	if err := h.swapGW.ApproveSwap(ctx, swapId, managerId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã hoàn tất duyệt đổi ca"})
	return nil
}

// CancelSwap
// @Summary      Cancel a sent swap request
// @Description  The requesting employee can cancel their swap request if it hasn't been approved yet.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Produce      json
// @Param        id   path   string true "Swap Request ID"
// @Success      204  {object} nil "No Content (Cancelled successfully)"
// @Failure      400  {object} object "Invalid ID or Bad Request"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shift-swaps/{id}/cancel [patch]
func (h *ShiftSwapHandler) CancelSwap(c *gin.Context) error {
	swapId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx := c.Request.Context()
	if err := h.swapGW.CancelSwap(ctx, swapId, userId); err != nil {
		return err
	}

	c.JSON(http.StatusNoContent, nil)
	return nil
}

// Release
// @Summary      Release a shift to the Shift Pool
// @Description  The employee releases an assigned shift so other staff members can apply for it.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Produce      json
// @Param        id   path   string true "Assignment ID"
// @Success      200  {object} map[string]interface{} "{"message": "Đã nhả ca thành công"}"
// @Failure      400  {object} object "Invalid ID or Bad Request"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts/{id}/releases [patch]
func (h *ShiftSwapHandler) Release(c *gin.Context) error {
	assignmentId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx := c.Request.Context()
	if err := h.swapGW.Release(ctx, assignmentId, userId); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã nhả ca thành công"})
	return nil
}

// GetAll
// @Summary      Get all shift swaps
// @Description  Retrieve a complete list of all shift swap requests (typically for managers).
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Produce      json
// @Param        query query  shift.GetAllSwapsParams false "Filter parameters"
// @Success      200   {array}  shift.SwapRequestItem "List of swap requests"
// @Failure      400   {object} object "Validation Error (Bad Request)"
// @Failure      401   {object} object "Unauthorized"
// @Failure      500   {object} object "Internal Server Error"
// @Router       /shift-swaps [get]
func (h *ShiftSwapHandler) GetAll(c *gin.Context) error {
	var params shift.GetAllSwapsParams
	if err := c.ShouldBindQuery(&params); err != nil {
		return validation.TranslateValidationError(err)
	}

	contextutils.ApplyStoreBoundaries(c, &params.StoreId)

	ctx := c.Request.Context()
	data, err := h.swapGW.GetAll(ctx, &params)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, data)
	return nil
}

// GetMySwaps
// @Summary      Get my shift swaps
// @Description  Retrieve all shift swap requests where the current logged-in user is either the sender or the receiver.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Produce      json
// @Success      200   {array}  shift.SwapRequestItem "List of user's swap requests"
// @Failure      401   {object} object "Unauthorized"
// @Failure      500   {object} object "Internal Server Error"
// @Router       /me/shift-swaps [get]
func (h *ShiftSwapHandler) GetMySwaps(c *gin.Context) error {
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx := c.Request.Context()
	data, err := h.swapGW.GetMySwaps(ctx, userId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, data)
	return nil
}
