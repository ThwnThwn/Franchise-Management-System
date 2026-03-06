package sales

import "github.com/google/wire"

type SalesHandlers struct {
	*OrderHandler
	*PaymentHandler
	*CartHandler

	*RefundTicketHandler
	*WalletHandler
	*WalletTransactionHandler
	*WithdrawalHandler
}

func NewSalesHandlers(
	order *OrderHandler,
	payment *PaymentHandler,
	cart *CartHandler,

	refund *RefundTicketHandler,
	wallet *WalletHandler,
	walletTransaction *WalletTransactionHandler,
	withdrawal *WithdrawalHandler,
) SalesHandlers {
	return SalesHandlers{
		OrderHandler:   order,
		PaymentHandler: payment,
		CartHandler:    cart,

		RefundTicketHandler:      refund,
		WalletHandler:            wallet,
		WalletTransactionHandler: walletTransaction,
		WithdrawalHandler:        withdrawal,
	}
}

var ProviderSet = wire.NewSet(
	NewOrderHandler,
	NewPaymentHandler,
	NewCartHandler,

	NewRefundTicketHandler,
	NewWalletHandler,
	NewWalletTransactionHandler,
	NewWithdrawalHandler,

	NewSalesHandlers,
)
