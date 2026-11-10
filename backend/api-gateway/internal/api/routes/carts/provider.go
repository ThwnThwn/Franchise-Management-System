package carts

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/sales"
	"api-gateway/internal/api/middleware"
)

type CartRouter struct {
	cfg    *config.Config
	authMW *middleware.AuthMiddleware
	cartHl *sales.CartHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	cartHl *sales.CartHandler,
) *CartRouter {
	return &CartRouter{
		cfg:    cfg,
		authMW: authMW,
		cartHl: cartHl,
	}
}
