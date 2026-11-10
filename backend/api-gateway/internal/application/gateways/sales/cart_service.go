package sales

import (
	"api-gateway/internal/domain/models/cart"
	"context"
)

type ICartService interface {
	GetMyCart(ctx context.Context) ([]*cart.CartItemReply, error)
	AddToMyCart(ctx context.Context, req *cart.UpsertCartItemRequest) (*cart.CartItemReply, error)
	UpdateMyCartItems(ctx context.Context, req *cart.UpsertCartItemRequest) (*cart.CartItemReply, error)
	DeleteMyCartItems(ctx context.Context, req *cart.DeleteCartItemsRequest) (bool, error)
	ClearMyCart(ctx context.Context) (bool, error)
}
