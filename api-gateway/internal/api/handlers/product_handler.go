package handlers

import (
	"api-gateway/internal/application/constants/errorcode"
	prodGW "api-gateway/internal/application/gateways/product"
	"api-gateway/internal/domain/models/product"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ProductHandler struct {
	productGW prodGW.IProductService
}

func NewProductHandler(productGW prodGW.IProductService) *ProductHandler {
	return &ProductHandler{productGW: productGW}
}

// GetProducts
// @Summary      Get All Products
// @Description  Retrieve a list of products with pagination and keyword search support.
// @Tags         Products
// @Accept       json
// @Produce      json
// @Param        query  query     product.GetProductsByKeywordRequest  false  "Filter options"
// @Success      200    {object}  map[string]interface{}               "List of products"
// @Failure      500    {object}  map[string]string                    "Internal server error"
// @Router       /products [get]
func (h *ProductHandler) GetProducts(c *gin.Context) {
	var req product.GetProductsByKeywordRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		c.Error(err)
		return
	}

	ctx := c.Request.Context()
	res, err := h.productGW.GetAllWithPagingBy(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// GetProductById
// @Summary      Get Product Details
// @Description  Retrieve full details of a specific product by its ID.
// @Tags         Products
// @Accept       json
// @Produce      json
// @Param        id   path      string  true  "Product UUID"
// @Success      200  {object}  map[string]interface{} "Product details"
// @Failure      400  {object}  map[string]string      "Invalid ID format"
// @Failure      404  {object}  map[string]string      "Product not found"
// @Router       /products/{id} [get]
func (h *ProductHandler) GetProductById(c *gin.Context) {
	id := c.Param("id")
	productId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	res, err := h.productGW.GetProductById(ctx, productId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// CreateProduct
// @Summary      Create New Product
// @Description  Add a new product to the catalog.
// @Tags         Products
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     product.CreateProductRequest  true  "Product details"
// @Success      200     {object} map[string]interface{}        "Success"
// @Failure      400     {object} map[string]string             "Validation error"
// @Router       /products [post]
func (h *ProductHandler) CreateProduct(c *gin.Context) {
	var req product.CreateProductRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	err := h.productGW.CreateProduct(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

// UpdateProduct
// @Summary      Update Product
// @Description  Update details of an existing product.
// @Tags         Products
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                        true  "Product UUID"
// @Param        request  body     product.UpdateProductRequest  true  "Updated details"
// @Success      200      {object} map[string]interface{}        "Success"
// @Failure      400      {object} map[string]string             "Validation error or Invalid ID"
// @Router       /products/{id} [patch]
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
	err = h.productGW.UpdateProduct(ctx, productId, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

// DeleteProduct
// @Summary      Delete Product
// @Description  Permanently remove a product from the catalog.
// @Tags         Products
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id   path      string                  true  "Product UUID"
// @Success      200  {object}  map[string]interface{}  "Success"
// @Failure      400  {object}  map[string]string       "Invalid ID format"
// @Router       /products/{id} [delete]
func (h *ProductHandler) DeleteProduct(c *gin.Context) {
	id := c.Param("id")
	productId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	err = h.productGW.DeleteProduct(ctx, productId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}
