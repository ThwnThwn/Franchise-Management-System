package engagement

import (
	"api-gateway/internal/domain/models/loyalty"
	"context"

	"github.com/google/uuid"
)

type ILoyaltyAccountService interface {
	GetByUserPhoneOrEmail(ctx context.Context, req *loyalty.GetByUserPhoneOrEmailRequest) (
		[]*loyalty.LoyaltyAccountResponse, error)

	GetLoyaltyReport(ctx context.Context, req *loyalty.GetLoyaltyReportRequest) (
		*loyalty.LoyaltyReportResponse, *loyalty.LoyaltyExportResponse, error)

	RedeemVoucher(ctx context.Context, voucherId uuid.UUID) error
}
