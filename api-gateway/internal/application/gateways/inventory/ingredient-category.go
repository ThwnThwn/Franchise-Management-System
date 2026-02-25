package inventory

import (
	"api-gateway/internal/domain/models/ingredient"
	"context"

	"github.com/google/uuid"
)

type IIngredientCategoryService interface {
	GetAll(ctx context.Context) ([]*ingredient.IngreCateViewRes, error)

	Create(ctx context.Context, req *ingredient.CreateIngreCateReq) error
	Update(ctx context.Context, id uuid.UUID, req *ingredient.UpdateIngreCateReq) error
	Delete(ctx context.Context, id uuid.UUID) error
}
