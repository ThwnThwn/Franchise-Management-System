package loyalty

type LoyaltyResponse struct {
	Id              string          `json:"id"`
	AvailablePoints string          `json:"availablePoints"`
	TotalSpent      string          `json:"totalSpent"`
	CurrentTier     *MembershipTier `json:"currentTier"`
}

type LoyaltyAccountResponse struct {
	LoyaltyResponse
	UserName string `json:"userName"`
}

type MembershipTier struct {
	Id               string  `json:"id"`
	Name             string  `json:"name"`
	MinSpendRequired string  `json:"minSpendRequired"`
	DiscountRate     float64 `json:"discountRate"`
	AmountPerPoint   string  `json:"amountPerPoint"`
	ValuePerPoint    string  `json:"valuePerPoint"`
}

type LoyaltyReportResponse struct {
	TotalMembers        int32               `json:"totalMembers"`
	TotalPointsEarned   float64             `json:"totalPointsEarned"`
	TotalPointsRedeemed float64             `json:"totalPointsRedeemed"`
	TierDistribution    []*TierDistribution `json:"tierDistribution"`
}

type LoyaltyExportResponse struct {
	FileName    string
	FileContent []byte
}

type TierDistribution struct {
	TierName    string `json:"tierName"`
	MemberCount int32  `json:"memberCount"`
}
