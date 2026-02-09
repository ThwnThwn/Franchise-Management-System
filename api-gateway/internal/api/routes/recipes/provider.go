package recipes

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
)

type RecipeRouter struct {
	cfg      *config.Config
	authMW   *middleware.AuthMiddleware
	recipeHl *handlers.RecipeHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	recipeHl *handlers.RecipeHandler,
) *RecipeRouter {
	return &RecipeRouter{
		cfg:      cfg,
		authMW:   authMW,
		recipeHl: recipeHl,
	}
}
