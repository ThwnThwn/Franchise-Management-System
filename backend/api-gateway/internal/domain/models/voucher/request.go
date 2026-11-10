package voucher

import "api-gateway/internal/domain/constants/enum"

type CreateVoucherRequest struct {
	Code             string           `json:"code"`
	Type             enum.VoucherType `json:"type"`
	DiscountValue    *string          `json:"discountValue"`
	MaxDiscountValue *string          `json:"maxDiscountValue"`
	MinOrderValue    *string          `json:"minOrderValue"`
	PointCost        *string          `json:"pointCost"`
	InitQuantity     *int32           `json:"initQuantity"`
	MaxUsagePerUser  *int32           `json:"maxUsagePerUser"`

	EndDate *string `json:"endDate"`

	VoucherItems []*CreateVoucherItemReq `json:"voucherItems"`
}

type CreateVoucherItemReq struct {
	Sku      string               `json:"sku"`
	Quantity int32                `json:"quantity"`
	Type     enum.VoucherItemType `json:"type"`
}

type UpdateVoucherRequest struct {
	DiscountValue    *string `json:"discountValue"`
	MaxDiscountValue *string `json:"maxDiscountValue"`
	MinOrderValue    *string `json:"minOrderValue"`
	PointCost        *string `json:"pointCost"`
	MaxUsagePerUser  *int32  `json:"maxUsagePerUser"`

	EndDate *string `json:"endDate"`

	VoucherItems []*UpdateVoucherItemReq `json:"voucherItems"`
}

type UpdateVoucherItemReq struct {
	Id       *string               `json:"id"`
	Sku      *string               `json:"sku"`
	Quantity *int32                `json:"quantity"`
	Type     *enum.VoucherItemType `json:"type"`
}

type UpdateVoucherStatusRequest struct {
	IsActive bool `json:"isActive"`
}
