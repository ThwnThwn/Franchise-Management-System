package inventory

import (
	inventoryGW "api-gateway/internal/application/gateways/inventory"
	"api-gateway/internal/domain/models/recipe"
	"context"

	inventoryPb "api-gateway/pkg/pb/inventory"

	"github.com/google/uuid"
)

type recipeGrpcClient struct {
	client inventoryPb.RecipeGrpcClient
}

func ProvideRecipeClient(f *clientFactory) inventoryGW.IRecipeService {
	return &recipeGrpcClient{client: inventoryPb.NewRecipeGrpcClient(f.conn)}
}

// GetAll implements inventory.IRecipeService.
func (r *recipeGrpcClient) GetAll(ctx context.Context, req *recipe.GetAllReq) ([]*recipe.RecipeViewRes, error) {
	res, err := r.client.GetByVariantIds(ctx, &inventoryPb.GetByVariantIdsRequest{
		VariantIds: req.VariantIds,
	})
	if err != nil {
		return []*recipe.RecipeViewRes{}, err
	}

	recipes := make([]*recipe.RecipeViewRes, 0, len(res.Items))
	for _, item := range res.Items {
		recipes = append(recipes, &recipe.RecipeViewRes{
			VariantId: item.VariantId,
			Name:      item.Name,
			BaseUnit:  item.BaseUnit,
			Quantity:  item.Quantity,
		})
	}
	return recipes, nil
}

// MutationVariantRecipe implements inventory.IRecipeService.
func (r *recipeGrpcClient) MutationVariantRecipe(ctx context.Context, id uuid.UUID, req []*recipe.RecipeIngredientRequest) error {
	ingresReq := make([]*inventoryPb.RecipeIngredientRequest, 0, len(req))
	for _, item := range req {
		ingresReq = append(ingresReq, &inventoryPb.RecipeIngredientRequest{
			IngredientId: item.IngredientId,
			Quantity:     item.Quantity,
		})
	}

	_, err := r.client.Mutation(ctx, &inventoryPb.MutationRecipeRequest{
		VariantId:   id.String(),
		Ingredients: ingresReq,
	})
	return err
}
