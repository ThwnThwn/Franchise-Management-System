package routes

import (
	"api-gateway/internal/api/routes/auth"
	"api-gateway/internal/api/routes/carts"
	"api-gateway/internal/api/routes/categories"
	"api-gateway/internal/api/routes/orders"
	"api-gateway/internal/api/routes/permissions"
	"api-gateway/internal/api/routes/products"
	"api-gateway/internal/api/routes/roles"
	"api-gateway/internal/api/routes/shifts"
	"api-gateway/internal/api/routes/stores"
	"api-gateway/internal/api/routes/users"
)

type RouterGroup struct {
	// Franchise
	Store stores.RouterGroup
	Shift shifts.RouterGroup

	// Identitt
	Auth     auth.RouterGroup
	User     users.RouterGroup
	Role     roles.RouterGroup
	RolePerm permissions.RouterGroup

	// Product
	Product  products.RouterGroup
	Category categories.RouterGroup

	// Sales
	Order orders.RouterGroup
	Cart  carts.RouterGroup

	// Inventory
}

var RouterGroupApp = new(RouterGroup)

var (
	// Franchise
	storeGroup = RouterGroupApp.Store
	shiftGroup = RouterGroupApp.Shift

	// Identity
	authGroup     = RouterGroupApp.Auth
	userGroup     = RouterGroupApp.User
	roleGroup     = RouterGroupApp.Role
	rolePermGroup = RouterGroupApp.RolePerm

	// Product
	productGroup  = RouterGroupApp.Product
	categoryGroup = RouterGroupApp.Category

	// Sales
	orderGroup = RouterGroupApp.Order
	cartGroup  = RouterGroupApp.Cart

	// Inventory
)
