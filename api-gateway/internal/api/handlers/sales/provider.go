package sales

import "github.com/google/wire"

type SalesHandlers struct {
	*OrderHandler
	*PaymentHandler
	*CartHandler
}

func NewSalesHandlers(
	order *OrderHandler,
	payment *PaymentHandler,
	cart *CartHandler,
) SalesHandlers {
	return SalesHandlers{
		OrderHandler:   order,
		PaymentHandler: payment,
		CartHandler:    cart,
	}
}

var ProviderSet = wire.NewSet(
	NewOrderHandler,
	NewPaymentHandler,
	NewCartHandler,
	NewSalesHandlers,
)
