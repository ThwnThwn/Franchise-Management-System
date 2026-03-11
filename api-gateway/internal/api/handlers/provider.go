package handlers

import (
	"api-gateway/internal/api/handlers/engagement"
	"api-gateway/internal/api/handlers/franchise"
	"api-gateway/internal/api/handlers/identity"
	"api-gateway/internal/api/handlers/inventory"
	"api-gateway/internal/api/handlers/product"
	"api-gateway/internal/api/handlers/sales"
	"api-gateway/internal/api/handlers/supplier"
	"api-gateway/internal/api/handlers/vnpay"

	"github.com/google/wire"
)

type AppHandlers struct {
	vnpay.VnPayHandlers

	engagement.EngagementHandlers

	// Franchise
	franchise.FranchiseHandlers

	// Identity
	identity.IdentityHandlers

	// Product
	product.ProductHandlers

	// Sales
	sales.SalesHandlers

	// Inventory
	inventory.InventoryHandlers

	// Supplier
	supplier.SupplierHandlers
}

// Gom nhóm tất cả Handler lại
var HandlerSet = wire.NewSet(
	vnpay.ProviderSet,

	engagement.ProviderSet,

	// Franchise
	franchise.ProviderSet,

	// Identity
	identity.ProviderSet,

	// Product
	product.ProviderSet,

	// Sales
	sales.ProviderSet,

	// Inventory
	inventory.ProviderSet,

	// Supplier
	supplier.ProviderSet,
)
