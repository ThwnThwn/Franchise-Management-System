package sales

import "github.com/google/wire"

var ProviderSet = wire.NewSet(
	NewOrderHandler,
	NewPaymentHandler,
	NewCartHandler,

	NewRefundTicketHandler,
	NewWalletHandler,
	NewWalletTransactionHandler,
	NewWithdrawalHandler,
)
