package handlers

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/constants/errorcode"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftHandler struct {
	shiftUC abstractions.IShiftUseCase
}

func NewShiftHandler(shiftUC abstractions.IShiftUseCase) *ShiftHandler {
	return &ShiftHandler{shiftUC: shiftUC}
}

func (h *ShiftHandler) GetUserCurrentShift(c *gin.Context) {
	userID, exists := c.Get("userID")
	if !exists {
		c.Error(errorcode.ErrInvalidToken)
		return
	}

	ctx := c.Request.Context()
	res, err := h.shiftUC.GetUserCurrentShift(ctx, userID.(uuid.UUID))
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}
