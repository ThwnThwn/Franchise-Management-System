package usecases

import (
	"api-gateway/internal/application/abstractions"
	productGW "api-gateway/internal/application/gateways/product"
	"api-gateway/internal/domain/entity"
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

// ExecuteGetProductById implements abstractions.IProductUseCase.
func (p *ProductUseCase) ExecuteGetProductById(ctx context.Context, id uuid.UUID) (*entity.Product, error) {
	return p.service.GetProductById(ctx, id)
}

// ExecuteGetProducts implements abstractions.IProductUseCase.
func (p *ProductUseCase) ExecuteGetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error) {
	return p.service.GetProducts(ctx, req)
}
