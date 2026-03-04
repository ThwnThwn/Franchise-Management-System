package voucher

type CreateVoucherRequest struct {
	Code             string  `json:"code"`
	Type             int32   `json:"type"`
	DiscountValue    *string `json:"discount_value"`
	MaxDiscountValue *string `json:"max_discount_value"`
	MinOrderValue    *string `json:"min_order_value"`
	PointCost        *string `json:"point_cost"`
	InitQuantity     *int32  `json:"init_quantity"`
	MaxUsagePerUser  *int32  `json:"max_usage_per_user"`

	EndDate *string `json:"end_date"`

	VoucherItems []*CreateVoucherItemReq `json:"voucher_items"`
}

type CreateVoucherItemReq struct {
	VariantId string `json:"variant_id"`
	Quantity  int32  `json:"quantity"`
	Type      int32  `json:"type"`
}

type UpdateVoucherRequest struct {
	DiscountValue    *string `json:"discount_value"`
	MaxDiscountValue *string `json:"max_discount_value"`
	MinOrderValue    *string `json:"min_order_value"`
	PointCost        *string `json:"point_cost"`
	MaxUsagePerUser  *int32  `json:"max_usage_per_user"`

	EndDate *string `json:"end_date"`

	VoucherItems []*UpdateVoucherItemReq `json:"voucher_items"`
}

type UpdateVoucherItemReq struct {
	Id        *string `json:"id"`
	VariantId *string `json:"variant_id"` // Đại diện cho Guid
	Quantity  *int32  `json:"quantity"`
	Type      *int32  `json:"type"`
}

type UpdateVoucherStatusRequest struct {
	IsActive bool `json:"is_active"`
}
