package handlers

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/inventory"
	"api-gateway/internal/domain/models/ingredient"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type StoreIngredientHandler struct {
	invenGW inventory.IStoreIngredientService
}

func NewStoreIngredientHandler(invenGW inventory.IStoreIngredientService) *StoreIngredientHandler {
	return &StoreIngredientHandler{invenGW: invenGW}
}

func (h *StoreIngredientHandler) IncreaseStock(c *gin.Context) {
	id := c.Param("id")
	storeId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req []*ingredient.IngredientStockItem
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	err = h.invenGW.IncreaseStock(ctx, storeId, req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

func (h *StoreIngredientHandler) GetAllStock(c *gin.Context) {
	var req ingredient.GetAllStockReq
	if err := c.ShouldBindQuery(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.invenGW.GetAllStock(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

func (h *StoreIngredientHandler) GetStockByStoreId(c *gin.Context) {
	id := c.Param("id")
	storeId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req ingredient.GetAllStockReq
	if err := c.ShouldBindQuery(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.invenGW.GetStockByStoreId(ctx, storeId, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}
