package usecases

import "github.com/google/wire"

var UseCaseSets = wire.NewSet(
	// Identity
	NewAuthUseCase,
	NewUserUseCase,

	// Product
	NewProductUseCase,
	NewCategoryUseCase,

	// Sales
	NewOrderUseCase,
)
