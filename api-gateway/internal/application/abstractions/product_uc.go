package abstractions

import (
	"api-gateway/internal/domain/models/product"
	"context"

	"github.com/google/uuid"
)

type IProductUseCase interface {
	GetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error)
	GetProductById(ctx context.Context, id uuid.UUID) (*product.Product, error)
}
