package sales

import (
	salesGW "api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/order"
	salesPb "api-gateway/pkg/pb/sales"
	"api-gateway/pkg/utils/timeutils"
	"context"

	"github.com/google/uuid"
	"github.com/jinzhu/copier"
)

type OrderGrpcClient struct {
	client salesPb.OrderGrpcClient
}

// ProvideOrderClient trích xuất Order Client từ Factory
func ProvideOrderClient(f *salesClientFactory) salesGW.IOrderService {
	return &OrderGrpcClient{client: salesPb.NewOrderGrpcClient(f.conn)}
}

// CreateOrder implements sales.IOrderService.
func (o *OrderGrpcClient) CreateOrder(ctx context.Context, req *order.CreateOrderRequest) (*order.CreateOrderReply, error) {
	pbItems := []*salesPb.CartItem{}

	copier.Copy(&pbItems, &req.Items)

	res, err := o.client.CreateOrder(ctx, &salesPb.CreateOrderRequest{
		Items:         pbItems,
		Tax:           req.Tax,
		PaymentMethod: req.PaymentMethod,
		Notes:         req.Notes,
	})

	if err != nil {
		return nil, err
	}
	return &order.CreateOrderReply{
		OrderId:     res.OrderId,
		OrderCode:   res.OrderCode,
		TotalAmount: res.TotalAmount,
	}, nil
}

// GetOrderById implements sales.IOrderService.
func (o *OrderGrpcClient) GetOrderById(ctx context.Context, id uuid.UUID) (*order.Order, error) {
	res, err := o.client.GetOrderById(ctx, &salesPb.GetOrderByIdRequest{Id: id.String()})
	if err != nil {
		return nil, err
	}
	items := []*order.OrderItem{}
	copier.Copy(&items, &res.Order.Items)

	order := &order.Order{
		Id:            res.Order.Id,
		Code:          res.Order.Code,
		Status:        res.Order.Status,
		TotalAmount:   res.Order.TotalAmount,
		Tax:           res.Order.Tax,
		PaymentMethod: res.Order.PaymentMethod,
		Notes:         res.Order.Notes,
		Items:         items,
		CreatedAt:     timeutils.StringToTime(res.Order.CreatedAt),
		UpdatedAt:     timeutils.StringToTime(res.Order.UpdatedAt),
	}

	return order, nil
}
