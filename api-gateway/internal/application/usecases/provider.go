package usecases

import "github.com/google/wire"

var UseCaseSets = wire.NewSet(
	// Cache
	NewBlacklistUseCase,

	// Franchise
	NewStoreUseCase,
	NewShiftUseCase,

	// Identity

	// Product
	NewProductUseCase,
	NewCategoryUseCase,

	// Sales
	NewOrderUseCase,
	NewCartUseCase,

	// Inventory
	NewInventoryUseCase,
)
