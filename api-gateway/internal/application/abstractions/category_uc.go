package abstractions

import (
	"api-gateway/internal/domain/models/category"
	"context"
)

type ICategoryUseCase interface {
	GetCategories(ctx context.Context) ([]category.Category, error)
}
