package usecases

import "github.com/google/wire"

var UseCaseSets = wire.NewSet(
	// Identity
	NewAuthUseCase,

	// Product
	NewProductUseCase,

	// Sales
	NewOrderUseCase,
)
