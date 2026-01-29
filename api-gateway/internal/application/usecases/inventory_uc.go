package usecases

import (
	"api-gateway/internal/application/abstractions"
	inventoryGW "api-gateway/internal/application/gateways/inventory"
	"api-gateway/internal/domain/models/ingredient"
	"context"
)

type InventoryUseCase struct {
	service inventoryGW.IInventoryService
}

func NewInventoryUseCase(service inventoryGW.IInventoryService) abstractions.IInventoryUseCase {
	return &InventoryUseCase{service: service}
}

// IncreaseStock implements abstractions.IInventoryUseCase.
func (i *InventoryUseCase) IncreaseStock(ctx context.Context, req *ingredient.IncreaseStockRequest) error {
	return i.service.IncreaseStock(ctx, req)
}
