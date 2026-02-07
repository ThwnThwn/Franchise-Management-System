package routes

import (
	"api-gateway/internal/api/routes/auth"
	"api-gateway/internal/api/routes/carts"
	"api-gateway/internal/api/routes/categories"
	"api-gateway/internal/api/routes/orders"
	"api-gateway/internal/api/routes/permissions"
	"api-gateway/internal/api/routes/products"
	"api-gateway/internal/api/routes/roles"
	"api-gateway/internal/api/routes/stores"
	"api-gateway/internal/api/routes/stores/ingredients"
	"api-gateway/internal/api/routes/users"

	"github.com/google/wire"
)

type AppRouter struct {
	// Franchise
	*stores.StoreRouter

	// Identity
	*auth.AuthRouter
	*users.UserRouter
	*roles.RoleRouter
	*permissions.PermissionRouter

	// Product
	*products.ProductRouter
	*categories.CategoryRouter

	// Sales
	*orders.OrderRouter
	*carts.CartRouter
}

var RouterSet = wire.NewSet(
	// Franchise
	stores.NewRouter,

	// Identity
	auth.NewRouter,
	users.NewRouter,
	roles.NewRouter,
	permissions.NewRouter,

	// Inventory
	ingredients.NewRouter,

	// Product
	products.NewRouter,
	categories.NewRouter,

	// Sales
	orders.NewRouter,
	carts.NewRouter,
)
