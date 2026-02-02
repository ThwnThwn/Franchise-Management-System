package usecases

import "github.com/google/wire"

var UseCaseSets = wire.NewSet(
	// Cache
	NewBlacklistUseCase,

	// Franchise
	NewStoreUseCase,
	NewShiftUseCase,

	// Identity
	NewAuthUseCase,
	NewUserUseCase,

	// Product
	NewProductUseCase,
	NewCategoryUseCase,

	// Sales
	NewOrderUseCase,
	NewCartUseCase,

	// Inventory
	NewInventoryUseCase,
)
