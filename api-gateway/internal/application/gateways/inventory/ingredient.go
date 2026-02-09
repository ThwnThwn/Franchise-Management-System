package inventory

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/ingredient"
	"context"

	"github.com/google/uuid"
)

type IIngredientService interface {
	GetAll(ctx context.Context, req *ingredient.GetAllIngredientReq,
	) (*common.PaginationResult[*ingredient.IngredientViewRes], error)

	Create(ctx context.Context, req *ingredient.CreateIngredientReq) error
	Update(ctx context.Context, id uuid.UUID, req *ingredient.UpdateIngredientReq) error
	Delete(ctx context.Context, id uuid.UUID) error
}
