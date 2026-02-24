package recipes

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/inventory"
	"api-gateway/internal/api/middleware"
)

type RecipeRouter struct {
	cfg      *config.Config
	authMW   *middleware.AuthMiddleware
	recipeHl *inventory.RecipeHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	recipeHl *inventory.RecipeHandler,
) *RecipeRouter {
	return &RecipeRouter{
		cfg:      cfg,
		authMW:   authMW,
		recipeHl: recipeHl,
	}
}
