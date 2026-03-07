package sales

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/order"
	"context"

	"github.com/google/uuid"
)

type IPaymentService interface {
	CreatePaymentUrl(ctx context.Context, id uuid.UUID, req *order.CreatePaymentRequest) (string, error)

	GetAll(ctx context.Context, req *order.GetAllPaymentsReq) (
		common.PaginationResult[*order.PaymentSummary], error)
	GetMyHistory(ctx context.Context, req *order.GetMyPaymentHistoryReq) (
		common.PaginationResult[*order.PaymentHistoryItem], error)

	ProcessVnPayReturn(ctx context.Context, queryParams map[string]string) (bool, string, error)
	ProcessVnPayIpn(ctx context.Context, queryParams map[string]string) (string, string, error)
}
