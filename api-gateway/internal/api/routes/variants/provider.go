package variants

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
)

type VariantRouter struct {
	cfg      *config.Config
	authMW   *middleware.AuthMiddleware
	recipeHl *handlers.RecipeHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	recipeHl *handlers.RecipeHandler,
) *VariantRouter {
	return &VariantRouter{
		cfg:      cfg,
		authMW:   authMW,
		recipeHl: recipeHl,
	}
}
