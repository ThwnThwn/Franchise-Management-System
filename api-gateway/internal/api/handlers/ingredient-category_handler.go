package handlers

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/inventory"
	"api-gateway/internal/domain/models/ingredient"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type IngredientCategoryHandler struct {
	ingreCateGW inventory.IIngredientCategoryService
}

func NewIngredientCategoryHandler(
	ingreCateGW inventory.IIngredientCategoryService,
) *IngredientCategoryHandler {
	return &IngredientCategoryHandler{
		ingreCateGW: ingreCateGW,
	}
}

// GetAll
// @Summary      Get All Ingredient Categories
// @Description  Retrieve a list of all ingredient categories.
// @Tags         Ingredient Categories
// @Accept       json
// @Produce      json
// @Success      200  {object}  map[string]interface{} "List of categories"
// @Failure      500  {object}  map[string]string      "Internal server error"
// @Router       /ingredients/categories [get]
func (h *IngredientCategoryHandler) GetAll(c *gin.Context) {
	ctx := c.Request.Context()
	res, err := h.ingreCateGW.GetAll(ctx)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// Create
// @Summary      Create Ingredient Category
// @Description  Create a new category for ingredients.
// @Tags         Ingredient Categories
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body ingredient.CreateIngreCateReq true "Category details"
// @Success      200     {object} map[string]interface{}    "Success"
// @Failure      400     {object} map[string]string         "Validation error"
// @Router       /ingredients/categories [post]
func (h *IngredientCategoryHandler) Create(c *gin.Context) {
	var req ingredient.CreateIngreCateReq
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	err := h.ingreCateGW.Create(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

// UpdateById
// @Summary      Update Ingredient Category
// @Description  Update an existing ingredient category by its ID.
// @Tags         Ingredient Categories
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                     true  "Category UUID"
// @Param        request  body     ingredient.UpdateIngreCateReq true "Updated details"
// @Success      200      {object} map[string]interface{}     "Success"
// @Failure      400      {object} map[string]string          "Validation error or Invalid ID"
// @Router       /ingredients/categories/{id} [patch]
func (h *IngredientCategoryHandler) UpdateById(c *gin.Context) {
	id := c.Param("id")
	ingreId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req ingredient.UpdateIngreCateReq
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	err = h.ingreCateGW.Update(ctx, ingreId, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

// DeleteById
// @Summary      Delete Ingredient Category
// @Description  Permanently remove an ingredient category.
// @Tags         Ingredient Categories
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id   path      string                  true  "Category UUID"
// @Success      200  {object}  map[string]interface{}  "Success"
// @Failure      400  {object}  map[string]string       "Invalid ID format"
// @Router       /ingredients/categories/{id} [delete]
func (h *IngredientCategoryHandler) DeleteById(c *gin.Context) {
	id := c.Param("id")
	ingreId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	err = h.ingreCateGW.Delete(ctx, ingreId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}
