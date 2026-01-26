package usecases

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/order"
	salesPb "api-gateway/pkg/pb/sales"
	"context"

	"github.com/google/uuid"
	"github.com/jinzhu/copier"
)

type OrderUseCase struct {
	service sales.IOrderService
}

func NewOrderUseCase(service sales.IOrderService) abstractions.IOrderUseCase {
	return &OrderUseCase{service: service}
}

// CreateOrder implements abstractions.IOrderUseCase.
func (o *OrderUseCase) CreateOrder(ctx context.Context, req order.CreateOrderRequest) (*order.CreateOrderReply, error) {
	pbItems := []*salesPb.CartItem{}

	copier.Copy(&pbItems, &req.Items)

	res, err := o.service.CreateOrder(ctx, &salesPb.CreateOrderRequest{
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

func (o *OrderUseCase) GetOrderById(ctx context.Context, id uuid.UUID) (*order.Order, error) {
	res, err := o.service.GetOrderById(ctx, &salesPb.GetOrderByIdRequest{Id: id.String()})
	if err != nil {
		return nil, err
	}
	order := &order.Order{}
	copier.Copy(order, res)
	return order, nil
}
