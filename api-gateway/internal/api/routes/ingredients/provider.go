package ingredients

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/inventory"
	"api-gateway/internal/api/middleware"
	"api-gateway/internal/api/routes/ingredients/categories"
)

type IngredientRouter struct {
	cfg          *config.Config
	authMW       *middleware.AuthMiddleware
	ingredientHl *inventory.IngredientHandler

	ingreCateR *categories.IngredientCategoryRouter
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	ingredientHl *inventory.IngredientHandler,
	ingreCateR *categories.IngredientCategoryRouter,
) *IngredientRouter {
	return &IngredientRouter{
		cfg:          cfg,
		authMW:       authMW,
		ingredientHl: ingredientHl,

		ingreCateR: ingreCateR,
	}
}
