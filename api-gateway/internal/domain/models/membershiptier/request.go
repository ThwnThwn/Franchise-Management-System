package membershiptier

type CreateMembershipTierRequest struct {
	Name             string  `json:"name"`
	MinSpendRequired string  `json:"minSpendRequired"`
	DiscountRate     float64 `json:"discountRate"`
	AmountPerPoint   string  `json:"amountPerPoint"`
	ValuePerPoint    string  `json:"valuePerPoint"`
}

type UpdateMembershipTierRequest struct {
	Name             *string  `json:"name"`
	MinSpendRequired *string  `json:"minSpendRequired"`
	DiscountRate     *float64 `json:"discountRate"`
	AmountPerPoint   *string  `json:"amountPerPoint"`
	ValuePerPoint    *string  `json:"valuePerPoint"`
}
