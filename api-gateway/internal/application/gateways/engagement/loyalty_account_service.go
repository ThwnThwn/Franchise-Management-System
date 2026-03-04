package engagement

import (
	"api-gateway/internal/domain/models/loyalty"
	"context"
)

type ILoyaltyAccountService interface {
	GetByUserPhoneOrEmail(ctx context.Context, req *loyalty.GetByUserPhoneOrEmailRequest) (
		[]*loyalty.LoyaltyAccountResponse, error)

	GetLoyaltyReport(ctx context.Context, req *loyalty.GetLoyaltyReportRequest) (
		*loyalty.LoyaltyReportResponse, error)
}
