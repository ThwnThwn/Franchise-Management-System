package engagement

import (
	"api-gateway/internal/domain/models/voucher"
	"context"

	"github.com/google/uuid"
)

type IVoucherService interface {
	GetAll(ctx context.Context) ([]*voucher.VoucherResponse, error)
	GetByCode(ctx context.Context, code string) (*voucher.VoucherResponse, error)
	GetMyVouchers(ctx context.Context) ([]*voucher.VoucherResponse, error)

	Create(ctx context.Context, req *voucher.CreateVoucherRequest) (*voucher.VoucherResponse, error)
	Update(ctx context.Context, id uuid.UUID, req *voucher.UpdateVoucherRequest) error
	UpdateStatus(ctx context.Context, id uuid.UUID, req *voucher.UpdateVoucherStatusRequest) error
	Delete(ctx context.Context, id uuid.UUID) error
}
