package withdrawals

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/sales"
	"api-gateway/internal/api/middleware"
)

type WithdrawalRouter struct {
	cfg          *config.Config
	authMW       *middleware.AuthMiddleware
	withdrawalHl *sales.WithdrawalHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	withdrawalHl *sales.WithdrawalHandler,
) *WithdrawalRouter {
	return &WithdrawalRouter{
		cfg:          cfg,
		authMW:       authMW,
		withdrawalHl: withdrawalHl,
	}
}
