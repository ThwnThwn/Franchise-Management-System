package supplier

import "github.com/google/wire"

var ProviderSet = wire.NewSet(
	NewSupplierHandler,
	NewPurchaseOrderHandler,
	NewSupplierPaymentHandler,

	// NewSupplierHandlers,
)

// type SupplierHandlers struct {
// 	*SupplierHandler
// 	*PurchaseOrderHandler
// 	*SupplierPaymentHandler
// }

// func NewSupplierHandlers(
// 	supplier *SupplierHandler,
// 	po *PurchaseOrderHandler,
// 	pm *SupplierPaymentHandler,
// ) SupplierHandlers {
// 	return SupplierHandlers{
// 		SupplierHandler:        supplier,
// 		PurchaseOrderHandler:   po,
// 		SupplierPaymentHandler: pm,
// 	}
// }
