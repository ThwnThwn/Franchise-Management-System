package inventory

import (
	inventoryGW "api-gateway/internal/application/gateways/inventory"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/ingredient"
	"context"

	inventoryPb "api-gateway/pkg/pb/inventory"

	"github.com/google/uuid"
)

type ingredientGrpcClient struct {
	client inventoryPb.IngredientGrpcClient
}

func ProvideIngredientClient(f *clientFactory) inventoryGW.IIngredientService {
	return &ingredientGrpcClient{client: inventoryPb.NewIngredientGrpcClient(f.conn)}
}

// GetAll implements inventory.IIngredientService.
func (i *ingredientGrpcClient) GetAll(ctx context.Context, req *ingredient.GetAllIngredientReq) (common.PaginationResult[*ingredient.IngredientViewRes], error) {
	res, err := i.client.GetAllIngredient(ctx, &inventoryPb.GetAllIngredientReq{
		CategoryId: req.CategoryId,
		Page:       req.Page,
		PageSize:   req.PageSize,
	})
	if err != nil {
		return common.PaginationResult[*ingredient.IngredientViewRes]{}, err
	}

	ingres := make([]*ingredient.IngredientViewRes, 0, len(res.IngredientRes))
	for _, item := range res.IngredientRes {
		ingres = append(ingres, &ingredient.IngredientViewRes{
			Id:         item.Id,
			Name:       item.Name,
			BaseUnit:   item.BaseUnit,
			CategoryId: item.CategoryId,
		})
	}
	return common.PaginationResult[*ingredient.IngredientViewRes]{
		Items:      ingres,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}

// Create implements inventory.IIngredientService.
func (i *ingredientGrpcClient) Create(ctx context.Context, req *ingredient.CreateIngredientReq) error {
	_, err := i.client.CreateIngredient(ctx, &inventoryPb.CreateIngredientReq{
		Name:       req.Name,
		BaseUnit:   req.BaseUnit,
		CategoryId: req.CategoryId,
	})
	return err
}

// Update implements inventory.IIngredientService.
func (i *ingredientGrpcClient) Update(ctx context.Context, id uuid.UUID, req *ingredient.UpdateIngredientReq) error {
	_, err := i.client.UpdateIngredient(ctx, &inventoryPb.UpdateIngredientReq{
		Id:         id.String(),
		Name:       req.Name,
		BaseUnit:   req.BaseUnit,
		CategoryId: req.CategoryId,
	})
	return err
}

// Delete implements inventory.IIngredientService.
func (i *ingredientGrpcClient) Delete(ctx context.Context, id uuid.UUID) error {
	_, err := i.client.DeleteIngredient(ctx, &inventoryPb.DeleteIngredientReq{
		Id: id.String(),
	})
	return err
}
