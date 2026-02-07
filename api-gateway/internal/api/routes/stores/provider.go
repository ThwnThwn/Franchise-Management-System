package stores

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/routes/stores/ingredients"
)

type StoreRouter struct {
	cfg     *config.Config
	storeHl *handlers.StoreHandler

	storeIngreR *ingredients.StoreIngredientRouter
}

func NewRouter(
	cfg *config.Config,
	storeHl *handlers.StoreHandler,

	storeIngreR *ingredients.StoreIngredientRouter,
) *StoreRouter {
	return &StoreRouter{
		cfg:     cfg,
		storeHl: storeHl,

		storeIngreR: storeIngreR,
	}
}
