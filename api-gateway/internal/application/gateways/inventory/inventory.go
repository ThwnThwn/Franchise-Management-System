package inventory

import (
	"api-gateway/internal/domain/models/ingredient"
	"context"
)

type IInventoryService interface {
	IncreaseStock(ctx context.Context, req *ingredient.IncreaseStockRequest) error
}
