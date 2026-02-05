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
