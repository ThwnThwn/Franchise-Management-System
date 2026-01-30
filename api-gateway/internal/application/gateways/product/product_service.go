package product

import (
	"api-gateway/internal/domain/models/product"
	"context"

	"github.com/google/uuid"
)

type IProductService interface {
	// Get product list with sizes and prices. Optional filter by categoryId.
	GetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error)
	// Get product details by id.
	GetProductById(ctx context.Context, id uuid.UUID) (*product.Product, error)
	CreateProduct(ctx context.Context, req *product.CreateProductRequest) error
	UpdateProduct(ctx context.Context, id uuid.UUID, req *product.UpdateProductRequest) error
	DeleteProduct(ctx context.Context, id uuid.UUID) error
}
