package payments

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/supplier"
	"api-gateway/internal/api/middleware"
)

type SupplierPaymentRouter struct {
	cfg       *config.Config
	authMW    *middleware.AuthMiddleware
	paymentHl *supplier.SupplierPaymentHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	paymentHl *supplier.SupplierPaymentHandler,
) *SupplierPaymentRouter {
	return &SupplierPaymentRouter{
		cfg:       cfg,
		authMW:    authMW,
		paymentHl: paymentHl,
	}
}
