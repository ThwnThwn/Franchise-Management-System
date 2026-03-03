package loyalty

type LoyaltyResponse struct {
	Id              string          `json:"id"`
	AvailablePoints string          `json:"availablePoints"`
	TotalSpent      string          `json:"totalSpent"`
	CurrentTier     *MembershipTier `json:"currentTier"`
}

type MembershipTier struct {
	Id               string  `json:"id"`
	Name             string  `json:"name"`
	MinSpendRequired string  `json:"minSpendRequired"`
	DiscountRate     float64 `json:"discountRate"`
	AmountPerPoint   string  `json:"amountPerPoint"`
	ValuePerPoint    string  `json:"valuePerPoint"`
}
