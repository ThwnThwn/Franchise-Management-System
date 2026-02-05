package sales

import (
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/order"
	"context"

	"github.com/google/uuid"
)

type IOrderService interface {
	GetOrderById(ctx context.Context, id uuid.UUID) (*order.Order, error)
	GetMyOrders(ctx context.Context, req *order.GetMyOrdersRequest) ([]*order.OrderSummary, error)
	CreateOrder(ctx context.Context, req *order.CreateOrderRequest) (*order.OrderReply, error)
	UpdateOrder(ctx context.Context, id uuid.UUID, items []*order.UpdateOrderItem) (*order.OrderReply, error)
	UpdateStatus(ctx context.Context, id uuid.UUID, status enum.OrderStatus) error
	CreatePaymentUrl(ctx context.Context, id uuid.UUID, req *order.CreatePaymentRequest) (string, error)
}
