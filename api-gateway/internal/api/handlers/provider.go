package handlers

import (
	"github.com/google/wire"
)

// Gom nhóm tất cả Handler lại
var HandlerSet = wire.NewSet(
	// Franchise
	NewStoreHandler,
	NewShiftHandler,

	// Identity
	NewAuthHandler,
	NewUserHandler,

	// Product
	NewProductHandler,
	NewCategoryHandler,

	// Sales
	NewOrderHandler,
	NewCartHandler,

	// Inventory
	NewInventoryHandler,
)

type AppHandlers struct {
	// Franchise
	*StoreHandler
	*ShiftHandler

	// Identity
	*AuthHandler
	*UserHandler

	// Product
	*ProductHandler
	*CategoryHandler

	// Sales
	*OrderHandler
	*CartHandler

	// Inventory
	*InventoryHandler
}
