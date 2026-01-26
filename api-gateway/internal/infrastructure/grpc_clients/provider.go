package grpc_clients

import (
	"api-gateway/internal/infrastructure/grpc_clients/identity"
	"api-gateway/internal/infrastructure/grpc_clients/product"
	"api-gateway/internal/infrastructure/grpc_clients/sales"

	"github.com/google/wire"
)

var ClientSet = wire.NewSet(
	// Identity Service
	identity.NewIdentityClientFactory,
	identity.ProvideAuthClient,

	// Product Service
	product.NewProductClient,

	// Sales Service
	sales.NewSalesClientFactory,
	sales.ProvideOrderClient,
)
