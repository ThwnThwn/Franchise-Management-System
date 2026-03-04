package sales

import (
	"api-gateway/internal/domain/models/order"
	"context"

	"github.com/google/uuid"
)

type IPaymentService interface {
	CreatePaymentUrl(ctx context.Context, id uuid.UUID, req *order.CreatePaymentRequest) (string, error)
	ProcessVnPayReturn(ctx context.Context, queryParams map[string]string) (bool, string, error)
}
