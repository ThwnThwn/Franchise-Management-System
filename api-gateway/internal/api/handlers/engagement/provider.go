package engagement

import "github.com/google/wire"

var ProviderSet = wire.NewSet(
	NewLoyaltyHandler,
	NewMembershipTierHandler,
	NewPointHandler,
	NewVoucherHandler,
	NewCampaignHandler,

	// NewEngagementHandlers,
)

// type EngagementHandlers struct {
// 	*LoyaltyHandler
// 	*MembershipTierHandler
// 	*PointHandler
// 	*VoucherHandler
// 	*CampaignHandler
// }

// func NewEngagementHandlers(
// 	loyaltyHandler *LoyaltyHandler,
// 	tierHandler *MembershipTierHandler,
// 	pointHandler *PointHandler,
// 	voucherHandler *VoucherHandler,
// 	campaignHl *CampaignHandler,
// ) EngagementHandlers {
// 	return EngagementHandlers{
// 		LoyaltyHandler:        loyaltyHandler,
// 		MembershipTierHandler: tierHandler,
// 		PointHandler:          pointHandler,
// 		VoucherHandler:        voucherHandler,
// 		CampaignHandler:       campaignHl,
// 	}
// }
