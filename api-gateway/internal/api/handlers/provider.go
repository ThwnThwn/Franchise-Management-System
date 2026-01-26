package handlers

import (
	"github.com/google/wire"
)

// Gom nhóm tất cả Handler lại
var HandlerSet = wire.NewSet(
	NewAuthHandler,
	NewUserHandler,

	NewProductHandler,
	NewCategoryHandler,

	NewOrderHandler,
)

type AppHandlers struct {
	*AuthHandler
	*UserHandler

	*ProductHandler
	*CategoryHandler

	*OrderHandler
}
