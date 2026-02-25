package sales

import "github.com/google/wire"

type SalesHandlers struct {
	*OrderHandler
	*CartHandler
}

func NewSalesHandlers(
	order *OrderHandler,
	cart *CartHandler,
) SalesHandlers {
	return SalesHandlers{
		OrderHandler: order,
		CartHandler:  cart,
	}
}

var ProviderSet = wire.NewSet(
	NewOrderHandler,
	NewCartHandler,
	NewSalesHandlers,
)
