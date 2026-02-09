package categories

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
)

type IngredientCategoryRouter struct {
	cfg         *config.Config
	authMW      *middleware.AuthMiddleware
	ingreCateHl *handlers.IngredientCategoryHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	ingreCateHl *handlers.IngredientCategoryHandler,
) *IngredientCategoryRouter {
	return &IngredientCategoryRouter{
		cfg:         cfg,
		authMW:      authMW,
		ingreCateHl: ingreCateHl,
	}
}
