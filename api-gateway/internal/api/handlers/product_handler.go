package handlers

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/domain/models/product"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ProductHandler struct {
	productUC abstractions.IProductUseCase
}

func NewProductHandler(productUC abstractions.IProductUseCase) *ProductHandler {
	return &ProductHandler{productUC: productUC}
}

func (h *ProductHandler) GetProducts(c *gin.Context) {
	var req product.GetProductsRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	res, err := h.productUC.GetProducts(ctx, &product.GetProductsRequest{
		CategoryId: req.CategoryId,
	})
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"products":   res.Products,
		"totalCount": res.TotalCount,
	})
}

func (h *ProductHandler) GetProductById(c *gin.Context) {
	id := c.Param("id")
	productId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	res, err := h.productUC.GetProductById(ctx, productId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}
