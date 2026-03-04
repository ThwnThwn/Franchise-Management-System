package inventory

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/inventory"
	"api-gateway/internal/domain/models/ingredient"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type IngredientHandler struct {
	ingreGW inventory.IIngredientService
}

func NewIngredientHandler(
	ingreGW inventory.IIngredientService,
) *IngredientHandler {
	return &IngredientHandler{
		ingreGW: ingreGW,
	}
}

// GetAll
// @Summary      Get All Ingredients
// @Description  Retrieve a list of ingredients with optional filtering and pagination.
// @Tags         Ingredients
// @Accept       json
// @Produce      json
// @Param        query  query     ingredient.GetAllIngredientReq  false  "Filter options"
// @Success      200    {object}  map[string]interface{}          "List of ingredients"
// @Failure      500    {object}  map[string]string               "Internal server error"
// @Router       /ingredients [get]
func (h *IngredientHandler) GetAll(c *gin.Context) error {
	var req ingredient.GetAllIngredientReq
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.ingreGW.GetAll(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Create
// @Summary      Create Ingredient
// @Description  Add a new ingredient to the inventory.
// @Tags         Ingredients
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     ingredient.CreateIngredientReq  true  "Ingredient details"
// @Success      200     {object} map[string]interface{}          "Success"
// @Failure      400     {object} map[string]string               "Validation error"
// @Router       /ingredients [post]
func (h *IngredientHandler) Create(c *gin.Context) error {
	var req ingredient.CreateIngredientReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	err := h.ingreGW.Create(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// UpdateById
// @Summary      Update Ingredient
// @Description  Update details of an existing ingredient by its ID.
// @Tags         Ingredients
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                          true  "Ingredient UUID"
// @Param        request  body     ingredient.UpdateIngredientReq  true  "Updated details"
// @Success      200      {object} map[string]interface{}          "Success"
// @Failure      400      {object} map[string]string               "Validation error or Invalid ID"
// @Router       /ingredients/{id} [patch]
func (h *IngredientHandler) UpdateById(c *gin.Context) error {
	id := c.Param("id")
	ingreId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req ingredient.UpdateIngredientReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	err = h.ingreGW.Update(ctx, ingreId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// DeleteById
// @Summary      Delete Ingredient
// @Description  Remove an ingredient from the inventory.
// @Tags         Ingredients
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id   path      string                  true  "Ingredient UUID"
// @Success      200  {object}  map[string]interface{}  "Success"
// @Failure      400  {object}  map[string]string       "Invalid ID format"
// @Router       /ingredients/{id} [delete]
func (h *IngredientHandler) DeleteById(c *gin.Context) error {
	id := c.Param("id")
	ingreId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()
	err = h.ingreGW.Delete(ctx, ingreId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}
