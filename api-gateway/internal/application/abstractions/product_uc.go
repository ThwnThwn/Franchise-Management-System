package abstractions

import (
	"api-gateway/pkg/pb/product"
	"context"

	"github.com/google/uuid"
)

type IProductUseCase interface {
	ExecuteGetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error)
	ExecuteGetProductById(ctx context.Context, id uuid.UUID) (*product.GetProductByIdReply, error)
}
