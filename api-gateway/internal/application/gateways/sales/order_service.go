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

	GetRevenueReport(ctx context.Context, req *order.GetRevenueReportRequest, includeCsv bool) (*order.GetRevenueReportReply, *order.CsvFile, error)
	GetOrderVolume(ctx context.Context, req *order.GetOrderVolumeRequest, includeCsv bool) (*order.GetOrderVolumeReply, *order.CsvFile, error)
	GetSalesTrend(ctx context.Context, req *order.GetSalesTrendRequest, includeCsv bool) (*order.GetSalesTrendReply, *order.CsvFile, error)

	AssignStaffToDelivery(ctx context.Context, orderId uuid.UUID, req *order.AssignStaffToDeliveryRequest) error
}
