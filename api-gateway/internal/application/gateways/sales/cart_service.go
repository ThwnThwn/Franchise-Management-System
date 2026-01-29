package sales

import (
	"api-gateway/internal/domain/models/cart"
	"context"
)

type ICartService interface {
	AddToCart(ctx context.Context, in *cart.UpsertCartItemRequest) (*cart.CartItemReply, error)
	UpdateCartItem(ctx context.Context, in *cart.UpsertCartItemRequest) (*cart.CartItemReply, error)
	DeleteListCartItem(ctx context.Context, in *cart.DeleteCartItemsRequest) (bool, error)
	GetCartItems(ctx context.Context) ([]*cart.CartItemReply, error)
	ClearCart(ctx context.Context) (bool, error)
}
