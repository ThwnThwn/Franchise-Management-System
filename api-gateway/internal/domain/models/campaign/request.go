package campaign

type CreateCampaignRequest struct {
	Name        string   `json:"name"`
	Description string   `json:"description"`
	StartDate   *string  `json:"startDate"`
	EndDate     *string  `json:"endDate"`
	VoucherIds  []string `json:"voucherIds"`
}

type UpdateActiveStatusRequest struct {
	IsActive bool `json:"isActive"`
}
