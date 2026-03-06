package sales

import (
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/order"
	"context"

	"github.com/google/uuid"
)

type IOrderService interface {
	GetOrders(ctx context.Context, req *order.GetOrdersRequest) (common.PaginationResult[*order.OrderSummary], error)
	GetOrderById(ctx context.Context, id uuid.UUID) (*order.Order, error)
	GetMyOrders(ctx context.Context, req *order.GetMyOrdersRequest) ([]*order.OrderSummary, error)
	CreateOrder(ctx context.Context, req *order.CreateOrderRequest) (*order.OrderReply, error)
	UpdateOrder(ctx context.Context, id uuid.UUID, items []*order.UpdateOrderItem) (*order.OrderReply, error)
	UpdateStatus(ctx context.Context, id uuid.UUID, status enum.OrderStatus) error
<<<<<<< HEAD
<<<<<<< HEAD
	CreatePaymentUrl(ctx context.Context, id uuid.UUID, req *order.CreatePaymentRequest) (string, error)
=======
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
=======
>>>>>>> 67a394a75859432ebb4e70c583813a2d5a7dff05

	GetRevenueReport(ctx context.Context, req *order.GetRevenueReportRequest, includeCsv bool) (*order.GetRevenueReportReply, *order.CsvFile, error)
	GetOrderVolume(ctx context.Context, req *order.GetOrderVolumeRequest, includeCsv bool) (*order.GetOrderVolumeReply, *order.CsvFile, error)
}
