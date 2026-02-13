package orders

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/sales"
	"api-gateway/internal/api/middleware"
)

type OrderRouter struct {
	authMW  *middleware.AuthMiddleware
	cfg     *config.Config
	orderHl *sales.OrderHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	orderHl *sales.OrderHandler,
) *OrderRouter {
	return &OrderRouter{
		cfg:     cfg,
		authMW:  authMW,
		orderHl: orderHl,
	}
}
