package enum

type VoucherType int32

const (
	VoucherType_DISCOUNT_PERCENTAGE   VoucherType = 0
	VoucherType_DISCOUNT_FIXED_AMOUNT VoucherType = 1
	VoucherType_SHIPPING_PERCENTAGE   VoucherType = 2
	VoucherType_SHIPPING_FIXED_AMOUNT VoucherType = 3
	VoucherType_GIFT                  VoucherType = 4
)

type VoucherItemType int32

const (
	VoucherItemType_ITEM_GIFT     VoucherItemType = 0
	VoucherItemType_ITEM_REQUIRED VoucherItemType = 1
)
