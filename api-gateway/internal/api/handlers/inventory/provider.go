package inventory

import "github.com/google/wire"

type InventoryHandlers struct {
	*StockHandler
	*IngredientHandler
	*IngredientCategoryHandler
	*RecipeHandler
}

func NewInventoryHandlers(
	stock *StockHandler,
	ingre *IngredientHandler,
	ingreCate *IngredientCategoryHandler,
	recipe *RecipeHandler,
) InventoryHandlers {
	return InventoryHandlers{
		StockHandler:              stock,
		IngredientHandler:         ingre,
		IngredientCategoryHandler: ingreCate,
		RecipeHandler:             recipe,
	}
}

var ProviderSet = wire.NewSet(
	NewStockHandler,
	NewIngredientHandler,
	NewIngredientCategoryHandler,
	NewRecipeHandler,

	NewInventoryHandlers,
)
