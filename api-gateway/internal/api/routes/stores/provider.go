package stores

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/franchise"
	"api-gateway/internal/api/routes/stores/stock"
)

type StoreRouter struct {
	cfg     *config.Config
	storeHl *franchise.StoreHandler

	stockR *stock.StockRouter
}

func NewRouter(
	cfg *config.Config,
	storeHl *franchise.StoreHandler,

	stockR *stock.StockRouter,
) *StoreRouter {
	return &StoreRouter{
		cfg:     cfg,
		storeHl: storeHl,

		stockR: stockR,
	}
}
