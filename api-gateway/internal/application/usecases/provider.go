package usecases

import "github.com/google/wire"

var UseCaseSets = wire.NewSet(
	NewCalcUseCase,

	// Identity
	NewAuthUseCase,

	// Product
	NewProductUseCase,

	// Sales
	NewOrderUseCase,
)
