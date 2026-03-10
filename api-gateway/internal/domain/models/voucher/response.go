package voucher

type VoucherResponse struct {
	Id   string `json:"id"`
	Code string `json:"code"`
	Type int32  `json:"type"`

	DiscountValue    *string `json:"discount_value"`
	MaxDiscountValue *string `json:"max_discount_value"`
	MinOrderValue    *string `json:"min_order_value"`
	PointCost        *string `json:"point_cost"`
	InitQuantity     *int32  `json:"init_quantity"`
	CurrentQuantity  *int32  `json:"current_quantity"`
	MaxUsagePerUser  *int32  `json:"max_usage_per_user"`
	IsActive         bool    `json:"is_active"`
	EndDate          *string `json:"end_date"`

	CreatedAt    *string                `json:"created_at"`
	UpdatedAt    *string                `json:"updated_at"`
	VoucherItems []*VoucherItemResponse `json:"voucher_items"`
}

type VoucherItemResponse struct {
	Id       string `json:"id"`
	Sku      string `json:"sku"`
	Quantity *int32 `json:"quantity"`
	Type     *int32 `json:"type"`
}
