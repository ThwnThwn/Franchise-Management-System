package enum

type SupplierPaymentMethod int32

const (
	PaymentMethodCash  SupplierPaymentMethod = 0
	PaymentMethodVNPay SupplierPaymentMethod = 1
)
