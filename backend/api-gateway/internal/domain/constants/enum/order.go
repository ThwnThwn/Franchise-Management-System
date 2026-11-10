package enum

type PaymentMethod int32

const (
	PaymentMethod_CASH PaymentMethod = iota
	PaymentMethod_VNPAY
)

type OrderStatus int32

const (
	OrderStatus_PAYMENT_PENDING OrderStatus = iota
	OrderStatus_CONFIRMATION_PENDING
	OrderStatus_CONFIRMED
	OrderStatus_PREPARING
	OrderStatus_READY_FOR_PICKUP
	OrderStatus_DELIVERING
	OrderStatus_COMPLETED
	OrderStatus_CANCEL
)

type OrderSaleItemType int32

const (
	OrderSaleItemType_VOUCHER   OrderSaleItemType = 0
	OrderSaleItemType_WALLET    OrderSaleItemType = 1
	OrderSaleItemType_POINT     OrderSaleItemType = 2
	OrderSaleItemType_PROMOTION OrderSaleItemType = 3
)

type OrderType int32

const (
	OrderType_Delivery OrderType = 0
	OrderType_Pickup   OrderType = 1
)
