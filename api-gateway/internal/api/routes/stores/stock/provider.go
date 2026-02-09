package stock

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
)

type StockRouter struct {
	cfg     *config.Config
	authMW  *middleware.AuthMiddleware
	stockHl *handlers.StockHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	stockHl *handlers.StockHandler,
) *StockRouter {
	return &StockRouter{
		cfg:     cfg,
		authMW:  authMW,
		stockHl: stockHl,
	}
}
