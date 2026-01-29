package usecases

import "github.com/google/wire"

var UseCaseSets = wire.NewSet(
	// Franchise
	NewStoreUseCase,
	NewShiftUseCasee,

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
