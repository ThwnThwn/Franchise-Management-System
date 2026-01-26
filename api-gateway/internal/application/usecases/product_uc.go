package usecases

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/gateways"
	"api-gateway/pkg/pb/product"
	"context"

	"github.com/google/uuid"
)

type ProductUseCase struct {
	service gateways.IProductService
}

func NewProductUseCase(service gateways.IProductService) abstractions.IProductUseCase {
	return &ProductUseCase{service: service}
}

// ExecuteGetProductById implements abstractions.IProductUseCase.
func (p *ProductUseCase) ExecuteGetProductById(ctx context.Context, id uuid.UUID) (*product.GetProductByIdReply, error) {
	req := &product.GetProductByIdRequest{
		Id: id.String(),
	}
	return p.service.GetProductById(ctx, req)
}

// ExecuteGetProducts implements abstractions.IProductUseCase.
func (p *ProductUseCase) ExecuteGetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error) {
	return p.service.GetProducts(ctx, req)
}
