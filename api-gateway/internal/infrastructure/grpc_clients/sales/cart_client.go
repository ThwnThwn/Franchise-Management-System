package sales

import (
	salesGW "api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/cart"
	salesPb "api-gateway/pkg/pb/sales"
	"context"
)

type CartGrpcClient struct {
	client salesPb.CartItemGrpcClient
}

// ProvideCartClient trích xuất Cart Client từ Factory
func ProvideCartClient(f *salesClientFactory) salesGW.ICartService {
	return &CartGrpcClient{
		client: salesPb.NewCartItemGrpcClient(f.conn),
	}
}

// GetCartItems implements sales.ICartService.
func (c *CartGrpcClient) GetCartItems(ctx context.Context) ([]*cart.CartItemReply, error) {
	panic("unimplemented")
}

// AddToCart implements sales.ICartService.
func (c *CartGrpcClient) AddToCart(ctx context.Context, in *cart.UpsertCartItemRequest) (*cart.CartItemReply, error) {
	panic("unimplemented")
}

// UpdateCartItem implements sales.ICartService.
func (c *CartGrpcClient) UpdateCartItem(ctx context.Context, in *cart.UpsertCartItemRequest) (*cart.CartItemReply, error) {
	panic("unimplemented")
}

// DeleteListCartItem implements sales.ICartService.
func (c *CartGrpcClient) DeleteListCartItem(ctx context.Context, in *cart.DeleteCartItemsRequest) (bool, error) {
	panic("unimplemented")
}

// ClearCart implements sales.ICartService.
func (c *CartGrpcClient) ClearCart(ctx context.Context) (bool, error) {
	panic("unimplemented")
}
