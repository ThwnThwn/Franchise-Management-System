package wallets

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/sales"
	"api-gateway/internal/api/middleware"
)

type WalletRouter struct {
	walletHl *sales.WalletHandler
	transHl  *sales.WalletTransactionHandler
	authMW   *middleware.AuthMiddleware
	cfg      *config.Config
}

func NewRouter(
	walletHl *sales.WalletHandler,
	transHl *sales.WalletTransactionHandler,
	authMW *middleware.AuthMiddleware,
	cfg *config.Config,
) *WalletRouter {
	return &WalletRouter{
		walletHl: walletHl,
		transHl:  transHl,
		authMW:   authMW,
		cfg:      cfg,
	}
}
