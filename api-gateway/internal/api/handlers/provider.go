package handlers

import (
	"api-gateway/internal/api/handlers/franchise"
	"api-gateway/internal/api/handlers/identity"
	"api-gateway/internal/api/handlers/inventory"
	"api-gateway/internal/api/handlers/product"
	"api-gateway/internal/api/handlers/sales"

	"github.com/google/wire"
)

type AppHandlers struct {
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
}

// Gom nhóm tất cả Handler lại
var HandlerSet = wire.NewSet(
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
)
