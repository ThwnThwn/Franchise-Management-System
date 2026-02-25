package product

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/product"
	"api-gateway/internal/domain/models/category"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type CategoryHandler struct {
	categoryGW product.ICategoryService
}

func NewCategoryHandler(categoryGW product.ICategoryService) *CategoryHandler {
	return &CategoryHandler{categoryGW: categoryGW}
}

// GetCategories
// @Summary      Get All Categories
// @Description  Retrieve a list of all available product categories.
// @Tags         Categories
// @Accept       json
// @Produce      json
// @Success      200  {object}  map[string]interface{} "List of categories"
// @Failure      500  {object}  map[string]string      "Internal server error"
// @Router       /categories [get]
func (h *CategoryHandler) GetCategories(c *gin.Context) {
	ctx := c.Request.Context()
	res, err := h.categoryGW.GetCategories(ctx)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// GetById
// @Summary      Get Category by ID
// @Description  Retrieve details of a specific category using its UUID.
// @Tags         Categories
// @Accept       json
// @Produce      json
// @Param        id   path      string  true  "Category UUID"
// @Success      200  {object}  map[string]interface{} "Category details"
// @Failure      400  {object}  map[string]string      "Invalid ID format"
// @Failure      404  {object}  map[string]string      "Category not found"
// @Router       /categories/{id} [get]
func (h *CategoryHandler) GetById(c *gin.Context) {
	id := c.Param("id")
	cateId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	res, err := h.categoryGW.GetById(ctx, cateId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// Create
// @Summary      Create Category
// @Description  Create a new product category.
// @Tags         Categories
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body category.CreateCategoryReq true "Category details"
// @Success      200  {object}  map[string]interface{} "Success"
// @Failure      400  {object}  map[string]string      "Validation error"
// @Router       /categories [post]
func (h *CategoryHandler) Create(c *gin.Context) {
	var req category.CreateCategoryReq
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	err := h.categoryGW.Create(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

// UpdateById
// @Summary      Update Category
// @Description  Update an existing category by its ID.
// @Tags         Categories
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path  string                    true  "Category UUID"
// @Param        request body  category.UpdateCategoryReq true "Updated details"
// @Success      200     {object}  map[string]interface{} "Success"
// @Failure      400     {object}  map[string]string      "Validation error or Invalid ID"
// @Router       /categories/{id} [patch]
func (h *CategoryHandler) UpdateById(c *gin.Context) {
	id := c.Param("id")
	cateId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req category.UpdateCategoryReq
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	err = h.categoryGW.Update(ctx, cateId, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

// DeleteById
// @Summary      Delete Category
// @Description  Permanently delete a category by its ID.
// @Tags         Categories
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id   path      string  true  "Category UUID"
// @Success      200  {object}  map[string]interface{} "Success"
// @Failure      400  {object}  map[string]string      "Invalid ID format"
// @Router       /categories/{id} [delete]
func (h *CategoryHandler) DeleteById(c *gin.Context) {
	id := c.Param("id")
	cateId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	err = h.categoryGW.Delete(ctx, cateId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}
