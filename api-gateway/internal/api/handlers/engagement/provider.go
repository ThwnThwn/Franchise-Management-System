package engagement

import "github.com/google/wire"

type EngagementHandlers struct {
	*LoyaltyHandler
	*MembershipTierHandler
	*PointLogHandler
	*VoucherHandler
}

func NewEngagementHandlers(
	loyaltyHandler *LoyaltyHandler,
	tierHandler *MembershipTierHandler,
	pointHandler *PointLogHandler,
	voucherHandler *VoucherHandler,
) EngagementHandlers {
	return EngagementHandlers{
		LoyaltyHandler:        loyaltyHandler,
		MembershipTierHandler: tierHandler,
		PointLogHandler:       pointHandler,
		VoucherHandler:        voucherHandler,
	}
}

var ProviderSet = wire.NewSet(
	NewLoyaltyHandler,
	NewMembershipTierHandler,
	NewPointLogHandler,
	NewVoucherHandler,

	NewEngagementHandlers,
)
