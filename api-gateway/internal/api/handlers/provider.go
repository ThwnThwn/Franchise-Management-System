package handlers

import "github.com/google/wire"

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
	*StockHandler
	*IngredientHandler
	*IngredientCategoryHandler
	*RecipeHandler
}

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
	NewStockHandler,
	NewIngredientHandler,
	NewIngredientCategoryHandler,
	NewRecipeHandler,
)
