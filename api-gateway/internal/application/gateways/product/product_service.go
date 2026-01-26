package product

import (
	"api-gateway/internal/domain/entity"
	"api-gateway/internal/domain/models/product"
	"context"

	"github.com/google/uuid"
)

type IProductService interface {
	// Get product list with sizes and prices. Optional filter by categoryId.
	GetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error)
	// Get product details by id.
	GetProductById(ctx context.Context, id uuid.UUID) (*entity.Product, error)
}
