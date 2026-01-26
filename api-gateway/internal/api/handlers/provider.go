package handlers

import (
	"github.com/google/wire"
)

// Gom nhóm tất cả Handler lại
var HandlerSet = wire.NewSet(
	NewAuthHandler,
	NewProductHandler,
	NewOrderHandler,
)

type AppHandlers struct {
	AuthHandler    *AuthHandler
	ProductHandler *ProductHandler
	OrderHandler   *OrderHandler
}
