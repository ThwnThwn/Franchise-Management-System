package handlers

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/domain/models/ingredient"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
)

type InventoryHandler struct {
	invenUC abstractions.IInventoryUseCase
}

func NewInventoryHandler(invenUC abstractions.IInventoryUseCase) *InventoryHandler {
	return &InventoryHandler{invenUC: invenUC}
}

func (h *InventoryHandler) IncreaseStock(c *gin.Context) {
	var req *ingredient.IncreaseStockRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	err := h.invenUC.IncreaseStock(ctx, req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}
