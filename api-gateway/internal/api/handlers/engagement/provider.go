package engagement

import "github.com/google/wire"

type EngagementHandlers struct {
	*LoyaltyHandler
	*MembershipTierHandler
	*PointHandler
	*VoucherHandler
}

func NewEngagementHandlers(
	loyaltyHandler *LoyaltyHandler,
	tierHandler *MembershipTierHandler,
	pointHandler *PointHandler,
	voucherHandler *VoucherHandler,
) EngagementHandlers {
	return EngagementHandlers{
		LoyaltyHandler:        loyaltyHandler,
		MembershipTierHandler: tierHandler,
		PointHandler:          pointHandler,
		VoucherHandler:        voucherHandler,
	}
}

var ProviderSet = wire.NewSet(
	NewLoyaltyHandler,
	NewMembershipTierHandler,
	NewPointHandler,
	NewVoucherHandler,

	NewEngagementHandlers,
)
