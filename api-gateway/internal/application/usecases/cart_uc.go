package usecases

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/cart"
	"context"
)

type CartUseCase struct {
	service sales.ICartService
}

func NewCartUseCase(service sales.ICartService) abstractions.ICartUseCase {
	return &CartUseCase{service: service}
}

// AddToCart implements abstractions.ICartUseCase.
func (c *CartUseCase) AddToCart(ctx context.Context, req *cart.UpsertCartItemRequest) (*cart.CartItemReply, error) {
	return c.service.AddToCart(ctx, req)
}

// ClearCart implements abstractions.ICartUseCase.
func (c *CartUseCase) ClearCart(ctx context.Context) (bool, error) {
	return c.service.ClearCart(ctx)
}

// DeleteListCartItem implements abstractions.ICartUseCase.
func (c *CartUseCase) DeleteListCartItem(ctx context.Context, req *cart.DeleteCartItemsRequest) (bool, error) {
	return c.service.DeleteListCartItem(ctx, req)
}

// GetCartItems implements abstractions.ICartUseCase.
func (c *CartUseCase) GetCartItems(ctx context.Context) ([]*cart.CartItemReply, error) {
	return c.service.GetCartItems(ctx)
}

// UpdateCartItem implements abstractions.ICartUseCase.
func (c *CartUseCase) UpdateCartItem(ctx context.Context, req *cart.UpsertCartItemRequest) (*cart.CartItemReply, error) {
	return c.service.UpdateCartItem(ctx, req)
}
