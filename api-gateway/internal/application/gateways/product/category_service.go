package product

import (
	"api-gateway/internal/domain/models/category"
	"context"

	"github.com/google/uuid"
)

type ICategoryService interface {
	GetCategories(ctx context.Context) ([]*category.Category, error)
	GetById(ctx context.Context, id uuid.UUID) (*category.Category, error)
	Create(ctx context.Context, req *category.CreateCategoryReq) error
	Update(ctx context.Context, id uuid.UUID, req *category.UpdateCategoryReq) error
	Delete(ctx context.Context, id uuid.UUID) error
}
