package handlers

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftHandler struct {
	shiftGW franchise.IShiftService
}

func NewShiftHandler(shiftGW franchise.IShiftService) *ShiftHandler {
	return &ShiftHandler{shiftGW: shiftGW}
}

// GetUserCurrentShift
// @Summary      Get My Current Shift
// @Description  Retrieve the current active shift for the logged-in user.
// @Tags         Shifts
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Success      200  {object}  map[string]interface{} "Current shift details"
// @Failure      401  {object}  map[string]string      "Unauthorized"
// @Failure      404  {object}  map[string]string      "No active shift found"
// @Router       /users/me/shifts/current [get]
func (h *ShiftHandler) GetUserCurrentShift(c *gin.Context) {
	userID, exists := c.Get("userID")
	if !exists {
		c.Error(errorcode.ErrInvalidToken)
		return
	}

	ctx := c.Request.Context()
	res, err := h.shiftGW.GetUserCurrentShift(ctx, userID.(uuid.UUID))
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}
