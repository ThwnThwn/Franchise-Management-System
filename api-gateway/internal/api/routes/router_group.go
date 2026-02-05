package routes

import (
	"api-gateway/internal/api/routes/auth"
	"api-gateway/internal/api/routes/cart"
	"api-gateway/internal/api/routes/category"
	"api-gateway/internal/api/routes/inventory"
	"api-gateway/internal/api/routes/order"
	"api-gateway/internal/api/routes/permission"
	"api-gateway/internal/api/routes/product"
	"api-gateway/internal/api/routes/role"
	"api-gateway/internal/api/routes/shift"
	"api-gateway/internal/api/routes/store"
	"api-gateway/internal/api/routes/user"
)

type RouterGroup struct {
	// Franchise
	Store store.RouterGroup
	Shift shift.RouterGroup

	// Identitt
	Auth     auth.RouterGroup
	User     user.RouterGroup
	Role     role.RouterGroup
	RolePerm permission.RouterGroup

	// Product
	Product  product.RouterGroup
	Category category.RouterGroup

	// Sales
	Order order.RouterGroup
	Cart  cart.RouterGroup

	// Inventory
	Inventory inventory.RouterGroup
}

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
	inventoryGroup = RouterGroupApp.Inventory
)
