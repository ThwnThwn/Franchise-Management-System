package gateways

import (
	"api-gateway/pkg/pb/product"
	"context"
)

type IProductService interface {
	// Get product list with sizes and prices. Optional filter by categoryId.
	GetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error)
	// Get product details by id.
	GetProductById(ctx context.Context, req *product.GetProductByIdRequest) (*product.GetProductByIdReply, error)
}
