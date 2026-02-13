package categories

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/product"
	"api-gateway/internal/api/middleware"
)

type CategoryRouter struct {
	cfg        *config.Config
	authMW     *middleware.AuthMiddleware
	categoryHl *product.CategoryHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	categoryHl *product.CategoryHandler,
) *CategoryRouter {
	return &CategoryRouter{
		cfg:        cfg,
		authMW:     authMW,
		categoryHl: categoryHl,
	}
}
