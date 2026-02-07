package ingredients

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
)

type StoreIngredientRouter struct {
	cfg          *config.Config
	authMW       *middleware.AuthMiddleware
	storeIngreHl *handlers.StoreIngredientHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	storeIngreHl *handlers.StoreIngredientHandler,
) *StoreIngredientRouter {
	return &StoreIngredientRouter{
		cfg:          cfg,
		authMW:       authMW,
		storeIngreHl: storeIngreHl,
	}
}
