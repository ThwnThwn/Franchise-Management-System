package handlers

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/constants/errorcode"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type UserHandler struct {
	userUC abstractions.IUserUseCase
}

func NewUserHandler(userUC abstractions.IUserUseCase) *UserHandler {
	return &UserHandler{userUC: userUC}
}

func (h *UserHandler) GetMe(c *gin.Context) {
	userID, exists := c.Get("userID")
	if !exists {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	res, err := h.userUC.GetById(ctx, userID.(uuid.UUID))
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}
