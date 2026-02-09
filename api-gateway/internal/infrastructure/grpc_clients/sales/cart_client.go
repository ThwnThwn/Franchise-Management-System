package sales

import (
	salesGW "api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/cart"
	"api-gateway/pkg/mapper"
	salesPb "api-gateway/pkg/pb/sales"
	"context"

	"google.golang.org/protobuf/types/known/emptypb"
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

// GetMyCart implements sales.ICartService.
func (c *CartGrpcClient) GetMyCart(ctx context.Context) ([]*cart.CartItemReply, error) {
	res, err := c.client.GetCartItems(ctx, &emptypb.Empty{})
	if err != nil {
		return nil, err
	}

	items := make([]*cart.CartItemReply, 0, len(res.Items))
	for _, item := range res.Items {
		items = append(items, mapper.MapCartItemReplyPbToModel(item))
	}

	return items, nil
}

// AddToMyCart implements sales.ICartService.
func (c *CartGrpcClient) AddToMyCart(ctx context.Context, req *cart.UpsertCartItemRequest) (*cart.CartItemReply, error) {
	res, err := c.client.AddToCart(ctx, &salesPb.AddToCartRequest{
		Sku:      req.Sku,
		Quantity: req.Quantity,
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapCartItemReplyPbToModel(res), nil
}

// UpdateMyCartItems implements sales.ICartService.
func (c *CartGrpcClient) UpdateMyCartItems(ctx context.Context, req *cart.UpsertCartItemRequest) (*cart.CartItemReply, error) {
	res, err := c.client.UpdateCartItem(ctx, &salesPb.UpdateCartItemRequest{
		Sku:      req.Sku,
		Quantity: req.Quantity,
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapCartItemReplyPbToModel(res), nil
}

// DeleteListCartItem implements sales.ICartService.
func (c *CartGrpcClient) DeleteMyCartItems(ctx context.Context, req *cart.DeleteCartItemsRequest) (bool, error) {
	res, err := c.client.DeleteListCartItem(ctx, &salesPb.DeleteListCartItemRequest{
		Skus: req.Skus,
	})
	return res.Success, err
}

// ClearCart implements sales.ICartService.
func (c *CartGrpcClient) ClearMyCart(ctx context.Context) (bool, error) {
	res, err := c.client.ClearCart(ctx, &emptypb.Empty{})
	return res.Success, err
}
