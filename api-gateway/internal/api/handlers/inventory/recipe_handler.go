package inventory

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/inventory"
	"api-gateway/internal/domain/models/recipe"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type RecipeHandler struct {
	recipeGW inventory.IRecipeService
}

func NewRecipeHandler(
	recipeGW inventory.IRecipeService,
) *RecipeHandler {
	return &RecipeHandler{
		recipeGW: recipeGW,
	}
}

// GetAll
// @Summary      Get All Recipes
// @Description  Retrieve a list of recipes with optional filtering.
// @Tags         Recipes
// @Accept       json
// @Produce      json
// @Param        query  query     recipe.GetAllReq  false  "Filter options"
// @Success      200    {object}  map[string]interface{} "List of recipes"
// @Failure      500    {object}  map[string]string      "Internal server error"
// @Router       /recipes [get]
func (h *RecipeHandler) GetAll(c *gin.Context) error {
	var req recipe.GetAllReq
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.recipeGW.GetAll(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// MutationVariantRecipe
// @Summary      Update Variant Recipe
// @Description  Define or update the list of ingredients (recipe) for a specific product variant.
// @Tags         Recipes
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                          true  "Variant UUID"
// @Param        request  body     []recipe.RecipeIngredientRequest true  "List of ingredients and quantities"
// @Success      200      {object} map[string]interface{}          "Success"
// @Failure      400      {object} map[string]string               "Validation error or Invalid ID"
// @Failure      401      {object} map[string]string               "Unauthorized"
// @Router       /variants/{id}/recipes [put]
func (h *RecipeHandler) MutationVariantRecipe(c *gin.Context) error {
	id := c.Param("id")
	variantId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req []*recipe.RecipeIngredientRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	err = h.recipeGW.MutationVariantRecipe(ctx, variantId, req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}
