package grpc_clients

import (
	"api-gateway/internal/infrastructure/grpc_clients/franchise"
	"api-gateway/internal/infrastructure/grpc_clients/identity"
	"api-gateway/internal/infrastructure/grpc_clients/inventory"
	"api-gateway/internal/infrastructure/grpc_clients/product"
	"api-gateway/internal/infrastructure/grpc_clients/sales"

	"github.com/google/wire"
)

var ClientSet = wire.NewSet(
	// Franchise Service
	franchise.NewFranchiseClientFactory,
	franchise.ProvideStoreClient,
	franchise.ProvideShiftClient,

	// Identity Service
	identity.NewIdentityClientFactory,
	identity.ProvideAuthClient,
	identity.ProvideUserClient,

	// Product Service
	product.NewProductClientFactory,
	product.ProvideCategoryClient,
	product.ProvideProductClient,

	// Sales Service
	sales.NewSalesClientFactory,
	sales.ProvideOrderClient,

	// Inventory Service
	inventory.NewInventoryClientFactory,
	inventory.ProvideInventoryClient,
)
