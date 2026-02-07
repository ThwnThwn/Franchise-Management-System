package inventory

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/ingredient"
	"context"

	"github.com/google/uuid"
)

type IStoreIngredientService interface {
	IncreaseStock(ctx context.Context, id uuid.UUID, req []*ingredient.IngredientStockItem) error

	GetAllStock(ctx context.Context, req *ingredient.GetAllStockReq,
	) (*common.PaginationResult[*ingredient.StockViewRes], error)

	GetStockByStoreId(ctx context.Context, id uuid.UUID, req *ingredient.GetAllStockReq,
	) (*common.PaginationResult[*ingredient.StockViewRes], error)
}
