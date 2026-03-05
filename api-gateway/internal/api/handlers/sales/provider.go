package sales

import "github.com/google/wire"

type SalesHandlers struct {
	*OrderHandler
	*PaymentHandler
	*CartHandler

	*RefundTicketHandler
	*WalletHandler
	*WalletTransactionHandler
}

func NewSalesHandlers(
	order *OrderHandler,
	payment *PaymentHandler,
	cart *CartHandler,

	refund *RefundTicketHandler,
	wallet *WalletHandler,
	walletTransaction *WalletTransactionHandler,
) SalesHandlers {
	return SalesHandlers{
		OrderHandler:   order,
		PaymentHandler: payment,
		CartHandler:    cart,

		RefundTicketHandler:      refund,
		WalletHandler:            wallet,
		WalletTransactionHandler: walletTransaction,
	}
}

var ProviderSet = wire.NewSet(
	NewOrderHandler,
	NewPaymentHandler,
	NewCartHandler,

	NewRefundTicketHandler,
	NewWalletHandler,
	NewWalletTransactionHandler,

	NewSalesHandlers,
)
