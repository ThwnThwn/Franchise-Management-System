package products

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/product"
	"api-gateway/internal/api/middleware"
)

type ProductRouter struct {
	cfg       *config.Config
	authMW    *middleware.AuthMiddleware
	productHl *product.ProductHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	productHl *product.ProductHandler,
) *ProductRouter {
	return &ProductRouter{
		cfg:       cfg,
		authMW:    authMW,
		productHl: productHl,
	}
}
