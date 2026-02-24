package variants

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/inventory"
	"api-gateway/internal/api/middleware"
)

type VariantRouter struct {
	cfg      *config.Config
	authMW   *middleware.AuthMiddleware
	recipeHl *inventory.RecipeHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	recipeHl *inventory.RecipeHandler,
) *VariantRouter {
	return &VariantRouter{
		cfg:      cfg,
		authMW:   authMW,
		recipeHl: recipeHl,
	}
}
