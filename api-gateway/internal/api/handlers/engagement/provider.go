package engagement

import "github.com/google/wire"

var ProviderSet = wire.NewSet(
	NewLoyaltyHandler,
	NewMembershipTierHandler,
	NewPointHandler,
	NewVoucherHandler,
	NewCampaignHandler,
)
