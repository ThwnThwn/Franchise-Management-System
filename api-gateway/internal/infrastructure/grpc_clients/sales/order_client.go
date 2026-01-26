package sales

import (
	salesGW "api-gateway/internal/application/gateways/sales"
	salesPb "api-gateway/pkg/pb/sales"
	"context"
)

type OrderGrpcClient struct {
	client salesPb.OrderGrpcClient
}

// ProvideOrderClient trích xuất Order Client từ Factory
func ProvideOrderClient(f *salesClientFactory) salesGW.IOrderService {
	return &OrderGrpcClient{client: salesPb.NewOrderGrpcClient(f.conn)}
}

// CreateOrder implements sales.IOrderService.
func (o *OrderGrpcClient) CreateOrder(ctx context.Context, req *salesPb.CreateOrderRequest) (*salesPb.CreateOrderReply, error) {
	res, err := o.client.CreateOrder(ctx, req)
	if err != nil {
		return nil, err
	}
	return res, nil
}

// GetOrderById implements sales.IOrderService.
func (o *OrderGrpcClient) GetOrderById(ctx context.Context, req *salesPb.GetOrderByIdRequest) (*salesPb.GetOrderByIdReply, error) {
	res, err := o.client.GetOrderById(ctx, req)
	if err != nil {
		return nil, err
	}
	return res, nil
}
