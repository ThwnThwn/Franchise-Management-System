package vouchers

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/engagement"
	"api-gateway/internal/api/middleware"
)

type VoucherRouter struct {
	cfg       *config.Config
	authMW    *middleware.AuthMiddleware
	voucherHl *engagement.VoucherHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	voucherHl *engagement.VoucherHandler,
) *VoucherRouter {
	return &VoucherRouter{
		cfg:       cfg,
		authMW:    authMW,
		voucherHl: voucherHl,
	}
}
