package stock

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/inventory"
	"api-gateway/internal/api/middleware"
)

type StockRouter struct {
	cfg     *config.Config
	authMW  *middleware.AuthMiddleware
	stockHl *inventory.StockHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	stockHl *inventory.StockHandler,
) *StockRouter {
	return &StockRouter{
		cfg:     cfg,
		authMW:  authMW,
		stockHl: stockHl,
	}
}
