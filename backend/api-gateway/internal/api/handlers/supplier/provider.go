package supplier

import "github.com/google/wire"

var ProviderSet = wire.NewSet(
	NewSupplierHandler,
	NewPurchaseOrderHandler,
	NewSupplierPaymentHandler,
)
