package inventory

import (
	inventoryGW "api-gateway/internal/application/gateways/inventory"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/ingredient"
	"context"

	inventoryPb "api-gateway/pkg/pb/inventory"

	"github.com/google/uuid"
)

type stockGrpcClient struct {
	client inventoryPb.InventoryGrpcClient
}

func ProvideStoreIngredientClient(f *clientFactory) inventoryGW.IStockService {
	return &stockGrpcClient{client: inventoryPb.NewInventoryGrpcClient(f.conn)}
}

// IncreaseStock implements inventory.IStockService.
func (s *stockGrpcClient) IncreaseStock(ctx context.Context, id uuid.UUID, req []*ingredient.IngredientStockItem) error {
	items := make([]*inventoryPb.IngredientStockItem, 0, len(req))
	for _, item := range req {
		items = append(items, &inventoryPb.IngredientStockItem{
			IngredientId: item.IngredientId,
			Quantity:     item.Quantity,
		})
	}

	_, err := s.client.IncreaseStock(ctx, &inventoryPb.IncreaseStockRequest{
		StoreId: id.String(),
		Items:   items,
	})
	if err != nil {
		return err
	}

	return nil
}

// GetAllStock implements inventory.IStockService.
func (s *stockGrpcClient) GetAllStock(ctx context.Context, req *ingredient.GetAllStockReq) (*common.PaginationResult[*ingredient.StockViewRes], error) {
	res, err := s.client.GetAllStock(ctx, &inventoryPb.GetAllStockReq{
		// StoreIds:      req.StoreIds,
		CategoryIds:   req.CategoryIds,
		StockStatus:   req.StockStatus,
		IngredientIds: req.IngredientIds,
		Page:          req.Page,
		PageSize:      req.PageSize,
	})

	if err != nil {
		return nil, err
	}

	stocks := make([]*ingredient.StockViewRes, 0, len(res.StockRes))
	for _, s := range res.StockRes {
		ingre := s.IngredientViewRes

		stocks = append(stocks, &ingredient.StockViewRes{
			StoreId: s.StoreId,
			IngredientViewRes: &ingredient.IngredientViewRes{
				Id:         ingre.Id,
				Name:       ingre.Name,
				BaseUnit:   ingre.BaseUnit,
				CategoryId: ingre.CategoryId,
			},
			Quantity:     s.MinThreshold,
			MinThreshold: s.MinThreshold,
			StockStatus:  s.StockStatus,
		})
	}

	return &common.PaginationResult[*ingredient.StockViewRes]{
		Items:      stocks,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}

// GetStockByStoreId implements inventory.IStockService.
func (s *stockGrpcClient) GetStockByStoreId(ctx context.Context, id uuid.UUID, req *ingredient.GetAllStockReq) (*common.PaginationResult[*ingredient.StockViewRes], error) {
	res, err := s.client.GetAllStock(ctx, &inventoryPb.GetAllStockReq{
		StoreIds:      []string{id.String()},
		CategoryIds:   req.CategoryIds,
		StockStatus:   req.StockStatus,
		IngredientIds: req.IngredientIds,
		Page:          req.Page,
		PageSize:      req.PageSize,
	})

	if err != nil {
		return nil, err
	}

	stocks := make([]*ingredient.StockViewRes, 0, len(res.StockRes))
	for _, s := range res.StockRes {
		ingre := s.IngredientViewRes

		stocks = append(stocks, &ingredient.StockViewRes{
			StoreId: s.StoreId,
			IngredientViewRes: &ingredient.IngredientViewRes{
				Id:         ingre.Id,
				Name:       ingre.Name,
				BaseUnit:   ingre.BaseUnit,
				CategoryId: ingre.CategoryId,
			},
			Quantity:     s.MinThreshold,
			MinThreshold: s.MinThreshold,
			StockStatus:  s.StockStatus,
		})
	}

	return &common.PaginationResult[*ingredient.StockViewRes]{
		Items:      stocks,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}
