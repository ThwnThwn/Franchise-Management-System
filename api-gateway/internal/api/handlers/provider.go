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
	NewRoleHandler,
	NewPermHandler,

	// Product
	NewProductHandler,
	NewCategoryHandler,

	// Sales
	NewOrderHandler,
	NewCartHandler,

	// Inventory
	NewStoreIngredientHandler,
)

type AppHandlers struct {
	// Franchise
	*StoreHandler
	*ShiftHandler

	// Identity
	*AuthHandler
	*UserHandler
	*RoleHandler
	*PermHandler

	// Product
	*ProductHandler
	*CategoryHandler

	// Sales
	*OrderHandler
	*CartHandler

	// Inventory
	*StoreIngredientHandler
}
