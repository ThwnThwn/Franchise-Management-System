package sales

import (
	"api-gateway/internal/domain/models/order"
	"context"

	"github.com/google/uuid"
)

type IOrderService interface {
	CreateOrder(ctx context.Context, req *order.CreateOrderRequest) (*order.CreateOrderReply, error)
	GetOrderById(ctx context.Context, id uuid.UUID) (*order.Order, error)
}
