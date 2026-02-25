package routes

import (
	"api-gateway/internal/api/routes/auth"
	"api-gateway/internal/api/routes/carts"
	"api-gateway/internal/api/routes/categories"
	"api-gateway/internal/api/routes/ingredients"
	ingreCate "api-gateway/internal/api/routes/ingredients/categories"
	"api-gateway/internal/api/routes/myshifts"
	"api-gateway/internal/api/routes/orders"
	"api-gateway/internal/api/routes/permissions"
	"api-gateway/internal/api/routes/products"
	"api-gateway/internal/api/routes/recipes"
	"api-gateway/internal/api/routes/roles"
	"api-gateway/internal/api/routes/shifts"
	"api-gateway/internal/api/routes/stores"
	"api-gateway/internal/api/routes/stores/stock"
	"api-gateway/internal/api/routes/users"
	"api-gateway/internal/api/routes/variants"

	"github.com/google/wire"
)

type AppRouter struct {
	// Franchise
	*stores.StoreRouter
	*myshifts.MyShiftsRouter
	*shifts.ShiftsRouter

	// Identity
	*auth.AuthRouter
	*users.UserRouter
	*roles.RoleRouter
	*permissions.PermissionRouter

	// Inventory
	*ingredients.IngredientRouter
	*recipes.RecipeRouter

	// Product
	*products.ProductRouter
	*categories.CategoryRouter
	*variants.VariantRouter

	// Sales
	*orders.OrderRouter
	*carts.CartRouter
}

var RouterSet = wire.NewSet(
	// Franchise
	stores.NewRouter,
	myshifts.NewRouter,
	shifts.NewRouter,

	// Identity
	auth.NewRouter,
	users.NewRouter,
	roles.NewRouter,
	permissions.NewRouter,

	// Inventory
	stock.NewRouter,
	ingredients.NewRouter,
	ingreCate.NewRouter,
	recipes.NewRouter,

	// Product
	products.NewRouter,
	categories.NewRouter,
	variants.NewRouter,

	// Sales
	orders.NewRouter,
	carts.NewRouter,
)
