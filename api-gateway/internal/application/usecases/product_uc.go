package usecases

import (
	"api-gateway/internal/application/abstractions"
	productGW "api-gateway/internal/application/gateways/product"
	"api-gateway/internal/domain/models/product"

	"context"

	"github.com/google/uuid"
)

type ProductUseCase struct {
	service productGW.IProductService
}

func NewProductUseCase(service productGW.IProductService) abstractions.IProductUseCase {
	return &ProductUseCase{service: service}
}

// GetProductById implements abstractions.IProductUseCase.
func (p *ProductUseCase) GetProductById(ctx context.Context, id uuid.UUID) (*product.Product, error) {
	return p.service.GetProductById(ctx, id)
}

// GetProducts implements abstractions.IProductUseCase.
func (p *ProductUseCase) GetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error) {
	return p.service.GetProducts(ctx, req)
}

// CreateProduct implements abstractions.IProductUseCase.
func (p *ProductUseCase) CreateProduct(ctx context.Context, req *product.CreateProductRequest) error {
	return p.CreateProduct(ctx, req)
}

// UpdateProduct implements abstractions.IProductUseCase.
func (p *ProductUseCase) UpdateProduct(ctx context.Context, id uuid.UUID, req *product.UpdateProductRequest) error {
	return p.UpdateProduct(ctx, id, req)
}

// DeleteProduct implements abstractions.IProductUseCase.
func (p *ProductUseCase) DeleteProduct(ctx context.Context, id uuid.UUID) error {
	return p.DeleteProduct(ctx, id)
}
