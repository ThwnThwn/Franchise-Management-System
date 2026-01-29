package abstractions

import (
	"api-gateway/internal/domain/models/ingredient"
	"context"
)

type IInventoryUseCase interface {
	IncreaseStock(ctx context.Context, req *ingredient.IncreaseStockRequest) error
}
