package routes

import (
	"api-gateway/internal/api/routes/auth"
	"api-gateway/internal/api/routes/category"
	"api-gateway/internal/api/routes/order"
	"api-gateway/internal/api/routes/product"
	"api-gateway/internal/api/routes/shift"
	"api-gateway/internal/api/routes/store"
	"api-gateway/internal/api/routes/user"
)

type RouterGroup struct {
	// Franchise
	Store store.RouterGroup
	Shift shift.RouterGroup

	// Identitt
	Auth auth.RouterGroup
	User user.RouterGroup

	// Product
	Product  product.RouterGroup
	Category category.RouterGroup

	// Sales
	Order order.RouterGroup
}

var (
	// Franchise
	storeGroup = RouterGroupApp.Store
	shiftGroup = RouterGroupApp.Shift

	// Identity
	authGroup = RouterGroupApp.Auth
	userGroup = RouterGroupApp.User

	// Product
	productGroup  = RouterGroupApp.Product
	categoryGroup = RouterGroupApp.Category

	// Sales
	orderGroup = RouterGroupApp.Order
)
