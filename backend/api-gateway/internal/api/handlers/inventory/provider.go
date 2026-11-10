package inventory

import "github.com/google/wire"

var ProviderSet = wire.NewSet(
	NewStockHandler,
	NewIngredientHandler,
	NewIngredientCategoryHandler,
	NewRecipeHandler,
)
