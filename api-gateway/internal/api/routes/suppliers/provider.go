package suppliers

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/supplier"
	"api-gateway/internal/api/middleware"
	"api-gateway/internal/api/routes/suppliers/payments"
	"api-gateway/internal/api/routes/suppliers/purchaseorders"
)

type SupplierRouter struct {
	cfg        *config.Config
	authMW     *middleware.AuthMiddleware
	supplierHl *supplier.SupplierHandler

	poRouter *purchaseorders.PurchaseOrderRouter
	pmRouter *payments.SupplierPaymentRouter
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	supplierHl *supplier.SupplierHandler,

	poRouter *purchaseorders.PurchaseOrderRouter,
	pmRouter *payments.SupplierPaymentRouter,
) *SupplierRouter {
	return &SupplierRouter{
		cfg:        cfg,
		authMW:     authMW,
		supplierHl: supplierHl,

		poRouter: poRouter,
		pmRouter: pmRouter,
	}
}
