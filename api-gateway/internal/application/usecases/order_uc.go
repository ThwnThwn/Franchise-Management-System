package usecases

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/order"
	"context"

	"github.com/google/uuid"
)

type OrderUseCase struct {
	service sales.IOrderService
}

func NewOrderUseCase(service sales.IOrderService) abstractions.IOrderUseCase {
	return &OrderUseCase{service: service}
}

// CreateOrder implements abstractions.IOrderUseCase.
func (o *OrderUseCase) CreateOrder(ctx context.Context, req *order.CreateOrderRequest) (*order.CreateOrderReply, error) {
	return o.service.CreateOrder(ctx, req)
}

func (o *OrderUseCase) GetOrderById(ctx context.Context, id uuid.UUID) (*order.Order, error) {
	return o.service.GetOrderById(ctx, id)
}

// GetMyOrders implements abstractions.IOrderUseCase.
func (o *OrderUseCase) GetMyOrders(ctx context.Context, req *order.GetMyOrdersRequest) ([]*order.OrderSummary, error) {
	return o.service.GetMyOrders(ctx, req)
}
