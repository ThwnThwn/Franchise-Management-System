package sales

import (
	"context"

	salesPb "api-gateway/pkg/pb/sales"
)

type IOrderService interface {
	CreateOrder(ctx context.Context, req *salesPb.CreateOrderRequest) (*salesPb.CreateOrderReply, error)
	GetOrderById(ctx context.Context, req *salesPb.GetOrderByIdRequest) (*salesPb.GetOrderByIdReply, error)
}
