package inventory

import (
	inventoryGW "api-gateway/internal/application/gateways/inventory"
	"api-gateway/internal/domain/models/ingredient"
	"context"

	inventoryPb "api-gateway/pkg/pb/inventory"

	"github.com/google/uuid"
	"google.golang.org/protobuf/types/known/emptypb"
)

type ingredientCategoryGrpcClient struct {
	client inventoryPb.IngredientCategoryGrpcClient
}

func ProvideIngredientCategoryClient(f *clientFactory) inventoryGW.IIngredientCategoryService {
	return &ingredientCategoryGrpcClient{client: inventoryPb.NewIngredientCategoryGrpcClient(f.conn)}
}

// GetAll implements inventory.IIngredientCategoryService.
func (i *ingredientCategoryGrpcClient) GetAll(ctx context.Context) ([]*ingredient.IngreCateViewRes, error) {
	res, err := i.client.GetAllCategories(ctx, &emptypb.Empty{})
	if err != nil {
		return []*ingredient.IngreCateViewRes{}, nil
	}

	cates := make([]*ingredient.IngreCateViewRes, 0, len(res.Categories))
	for _, item := range res.Categories {
		cates = append(cates, &ingredient.IngreCateViewRes{
			Id:          item.Id,
			Name:        item.Name,
			Description: item.Description,
		})
	}

	return cates, nil
}

// Create implements inventory.IIngredientCategoryService.
func (i *ingredientCategoryGrpcClient) Create(ctx context.Context, req *ingredient.CreateIngreCateReq) error {
	_, err := i.client.CreateCategory(ctx, &inventoryPb.CreateCategoryRequest{
		Name:        req.Name,
		Description: req.Description,
	})
	return err
}

// Update implements inventory.IIngredientCategoryService.
func (i *ingredientCategoryGrpcClient) Update(ctx context.Context, id uuid.UUID, req *ingredient.UpdateIngreCateReq) error {
	_, err := i.client.UpdateCategory(ctx, &inventoryPb.UpdateCategoryRequest{
		Id:          id.String(),
		Name:        req.Name,
		Description: req.Description,
	})
	return err
}

// Delete implements inventory.IIngredientCategoryService.
func (i *ingredientCategoryGrpcClient) Delete(ctx context.Context, id uuid.UUID) error {
	_, err := i.client.DeleteCategory(ctx, &inventoryPb.DeleteCategoryRequest{
		Id: id.String(),
	})
	return err
}
