package refunds

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/sales"
	"api-gateway/internal/api/middleware"
)

type RefundRouter struct {
	refundHl *sales.RefundTicketHandler
	authMW   *middleware.AuthMiddleware
	cfg      *config.Config
}

func NewRouter(
	refundHl *sales.RefundTicketHandler,
	authMW *middleware.AuthMiddleware,
	cfg *config.Config,
) *RefundRouter {
	return &RefundRouter{
		refundHl: refundHl,
		authMW:   authMW,
		cfg:      cfg,
	}
}
