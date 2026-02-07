package orders

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
)

type OrderRouter struct {
	authMW  *middleware.AuthMiddleware
	cfg     *config.Config
	orderHl *handlers.OrderHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	orderHl *handlers.OrderHandler,
) *OrderRouter {
	return &OrderRouter{
		cfg:     cfg,
		authMW:  authMW,
		orderHl: orderHl,
	}
}
