package routes

import (
	"api-gateway/internal/api/routes/auth"
	"api-gateway/internal/api/routes/carts"
	"api-gateway/internal/api/routes/categories"
	"api-gateway/internal/api/routes/ingredients"
	ingreCate "api-gateway/internal/api/routes/ingredients/categories"
	"api-gateway/internal/api/routes/loyalty"
	"api-gateway/internal/api/routes/membershiptiers"
	"api-gateway/internal/api/routes/myshifts"
	"api-gateway/internal/api/routes/orders"
	"api-gateway/internal/api/routes/permissions"
	"api-gateway/internal/api/routes/points"
	"api-gateway/internal/api/routes/products"
	"api-gateway/internal/api/routes/recipes"
	"api-gateway/internal/api/routes/refunds"
	"api-gateway/internal/api/routes/roles"
	"api-gateway/internal/api/routes/shifts"
	"api-gateway/internal/api/routes/stores"
	"api-gateway/internal/api/routes/stores/stock"
	"api-gateway/internal/api/routes/users"
	"api-gateway/internal/api/routes/variants"
	"api-gateway/internal/api/routes/vouchers"
	"api-gateway/internal/api/routes/wallets"
	"api-gateway/internal/api/routes/withdrawals"

	"github.com/google/wire"
)

type AppRouter struct {
	// Engagement
	*loyalty.LoyaltyRouter
	*membershiptiers.MembershipTierRouter
	*points.PointLogRouter
	*vouchers.VoucherRouter

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
	*wallets.WalletRouter
	*refunds.RefundRouter
	*withdrawals.WithdrawalRouter
}

var RouterSet = wire.NewSet(
	// Engagement
	loyalty.NewRouter,
	membershiptiers.NewRouter,
	points.NewRouter,
	vouchers.NewRouter,

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
	wallets.NewRouter,
	refunds.NewRouter,
	withdrawals.NewRouter,
)
