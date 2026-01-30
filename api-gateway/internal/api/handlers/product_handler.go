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
	if err := c.ShouldBindQuery(&req); err != nil {
		c.Error(err)
		return
	}

	ctx := c.Request.Context()
	res, err := h.productUC.GetProducts(ctx, &req)
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

func (h *ProductHandler) CreateProduct(c *gin.Context) {
	var req product.CreateProductRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	err := h.productUC.CreateProduct(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

func (h *ProductHandler) UpdateProduct(c *gin.Context) {
	var req product.UpdateProductRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	id := c.Param("id")
	productId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	err = h.productUC.UpdateProduct(ctx, productId, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

func (h *ProductHandler) DeleteProduct(c *gin.Context) {
	id := c.Param("id")
	productId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	err = h.productUC.DeleteProduct(ctx, productId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}
