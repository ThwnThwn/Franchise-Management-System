package purchaseorders

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/supplier"
	"api-gateway/internal/api/middleware"
)

type PurchaseOrderRouter struct {
	cfg       *config.Config
	authMW    *middleware.AuthMiddleware
	poHl      *supplier.PurchaseOrderHandler
	paymentHl *supplier.SupplierPaymentHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	poHl *supplier.PurchaseOrderHandler,
	paymentHl *supplier.SupplierPaymentHandler,
) *PurchaseOrderRouter {
	return &PurchaseOrderRouter{
		cfg:       cfg,
		authMW:    authMW,
		poHl:      poHl,
		paymentHl: paymentHl,
	}
}
