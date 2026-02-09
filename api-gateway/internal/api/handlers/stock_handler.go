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

type StockHandler struct {
	stockGW inventory.IStockService
}

func NewStockHandler(stockGW inventory.IStockService) *StockHandler {
	return &StockHandler{stockGW: stockGW}
}

// GetAllStock
// @Summary      Get All Global Stock
// @Description  Retrieve inventory levels of ingredients across all stores (Management view).
// @Tags         Store Stock
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     ingredient.GetAllStockReq  false  "Filter options"
// @Success      200    {object}  map[string]interface{}     "Global inventory list"
// @Failure      500    {object}  map[string]string          "Internal server error"
// @Router       /stores/stock [get]
func (h *StockHandler) GetAllStock(c *gin.Context) {
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

	res, err := h.stockGW.GetAllStock(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// GetStockByStoreId
// @Summary      Get Store Stock
// @Description  Retrieve the current inventory of ingredients for a specific store.
// @Tags         Store Stock
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id     path      string                     true   "Store UUID"
// @Param        query  query     ingredient.GetAllStockReq  false  "Filter options"
// @Success      200    {object}  map[string]interface{}     "Store inventory list"
// @Failure      400    {object}  map[string]string          "Invalid ID format"
// @Failure      404    {object}  map[string]string          "Store not found"
// @Router       /stores/{id}/stock [get]
func (h *StockHandler) GetStockByStoreId(c *gin.Context) {
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

	res, err := h.stockGW.GetStockByStoreId(ctx, storeId, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// IncreaseStock
// @Summary      Increase Ingredient Stock
// @Description  Add quantity to specific ingredients in a store's inventory.
// @Tags         Store Stock
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                            true  "Store UUID"
// @Param        request  body     []ingredient.IngredientStockItem  true  "List of ingredients to add"
// @Success      200      {object} map[string]interface{}            "Success"
// @Failure      400      {object} map[string]string                 "Validation error or Invalid ID"
// @Failure      401      {object} map[string]string                 "Unauthorized"
// @Router       /stores/{id}/stock [put]
func (h *StockHandler) IncreaseStock(c *gin.Context) {
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

	err = h.stockGW.IncreaseStock(ctx, storeId, req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}
