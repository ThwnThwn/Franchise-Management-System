package voucher

type VoucherResponse struct {
	Id   string `json:"id"`
	Code string `json:"code"`
	Type int32  `json:"type"`

	DiscountValue    *string `json:"discountValue"`
	MaxDiscountValue *string `json:"maxDiscountValue"`
	MinOrderValue    *string `json:"minOrderValue"`
	PointCost        *string `json:"pointCost"`
	InitQuantity     *int32  `json:"initQuantity"`
	CurrentQuantity  *int32  `json:"currentQuantity"`
	MaxUsagePerUser  *int32  `json:"maxUsagePerUser"`
	IsActive         bool    `json:"isActive"`
	EndDate          *string `json:"endDate"`

	CreatedAt    *string                `json:"createdAt"`
	UpdatedAt    *string                `json:"updatedAt"`
	VoucherItems []*VoucherItemResponse `json:"voucherItems"`
}

type VoucherItemResponse struct {
	Id       string `json:"id"`
	Sku      string `json:"sku"`
	Quantity *int32 `json:"quantity"`
	Type     *int32 `json:"type"`
}
