package product

import (
	"api-gateway/internal/domain/models/category"
	"context"
)

type ICategoryService interface {
	GetCategories(ctx context.Context) ([]category.Category, error)
}
