package handlers

import (
	"api-gateway/internal/application/gateways/inventory"
	"api-gateway/internal/domain/models/ingredient"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
)

type InventoryHandler struct {
	invenGW inventory.IInventoryService
}

func NewInventoryHandler(invenGW inventory.IInventoryService) *InventoryHandler {
	return &InventoryHandler{invenGW: invenGW}
}

func (h *InventoryHandler) IncreaseStock(c *gin.Context) {
	var req *ingredient.IncreaseStockRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	err = h.invenGW.IncreaseStock(ctx, req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}
