package membershiptier

type MembershipTierResponse struct {
	Id               string  `json:"id"`
	Name             string  `json:"name"`
	MinSpendRequired string  `json:"minSpendRequired"` // Dùng string để mapping với decimal
	DiscountRate     float64 `json:"discountRate"`
	AmountPerPoint   string  `json:"amountPerPoint"`
	ValuePerPoint    string  `json:"valuePerPoint"`
}
