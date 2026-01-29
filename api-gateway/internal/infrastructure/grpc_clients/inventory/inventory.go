package inventory

import (
	inventoryGW "api-gateway/internal/application/gateways/inventory"
	"api-gateway/internal/domain/models/ingredient"
	"context"

	inventoryPb "api-gateway/pkg/pb/inventory"
)

type inventoryGrpcClient struct {
	client inventoryPb.InventoryGrpcClient
}

func ProvideInventoryClient(f *clientFactory) inventoryGW.IInventoryService {
	return &inventoryGrpcClient{client: inventoryPb.NewInventoryGrpcClient(f.conn)}
}

// IncreaseStock implements inventory.IInventoryService.
func (i *inventoryGrpcClient) IncreaseStock(ctx context.Context, req *ingredient.IncreaseStockRequest) error {
	items := make([]*inventoryPb.IngredientStockItem, 0, len(req.Items))
	for _, item := range req.Items {
		items = append(items, &inventoryPb.IngredientStockItem{
			IngredientId: item.IngredientId,
			Quantity:     item.Quantity,
		})
	}

	_, err := i.client.IncreaseStock(ctx, &inventoryPb.IncreaseStockRequest{
		StoreId: req.StoreId,
		Items:   items,
	})
	if err != nil {
		return err
	}

	return nil
}
