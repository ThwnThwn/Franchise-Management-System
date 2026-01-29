package abstractions

import (
	"api-gateway/internal/domain/models/cart"
	"context"
)

type ICartUseCase interface {
	AddToCart(ctx context.Context, req *cart.UpsertCartItemRequest) (*cart.CartItemReply, error)
	UpdateCartItem(ctx context.Context, req *cart.UpsertCartItemRequest) (*cart.CartItemReply, error)
	DeleteListCartItem(ctx context.Context, req *cart.DeleteCartItemsRequest) (bool, error)
	GetCartItems(ctx context.Context) ([]*cart.CartItemReply, error)
	ClearCart(ctx context.Context) (bool, error)
}
