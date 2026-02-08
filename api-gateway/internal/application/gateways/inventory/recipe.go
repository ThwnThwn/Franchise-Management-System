package inventory

import (
	"api-gateway/internal/domain/models/recipe"
	"context"

	"github.com/google/uuid"
)

type IRecipeService interface {
	GetAll(ctx context.Context, req *recipe.GetAllReq) ([]*recipe.RecipeViewRes, error)
	MutationVariantRecipe(ctx context.Context, id uuid.UUID,
		req []*recipe.RecipeIngredientRequest) error
}
