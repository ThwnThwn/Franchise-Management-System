package enum

type PaymentMethod int32

const (
	PaymentMethod_CASH PaymentMethod = iota
	PaymentMethod_VNPAY
)

type OrderStatus int32

const (
	OrderStatus_PAYMENT_PENDING OrderStatus = iota
	OrderStatus_PREPARING
	OrderStatus_READY
	OrderStatus_COMPLETED
	OrderStatus_CANCEL
)
