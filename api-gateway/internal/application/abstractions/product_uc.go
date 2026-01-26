package abstractions

import (
	"api-gateway/internal/domain/entity"
	"api-gateway/internal/domain/models/product"
	"context"

	"github.com/google/uuid"
)

type IProductUseCase interface {
	ExecuteGetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error)
	ExecuteGetProductById(ctx context.Context, id uuid.UUID) (*entity.Product, error)
}
