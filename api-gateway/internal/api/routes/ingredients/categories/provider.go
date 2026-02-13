package categories

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/inventory"
	"api-gateway/internal/api/middleware"
)

type IngredientCategoryRouter struct {
	cfg         *config.Config
	authMW      *middleware.AuthMiddleware
	ingreCateHl *inventory.IngredientCategoryHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	ingreCateHl *inventory.IngredientCategoryHandler,
) *IngredientCategoryRouter {
	return &IngredientCategoryRouter{
		cfg:         cfg,
		authMW:      authMW,
		ingreCateHl: ingreCateHl,
	}
}
