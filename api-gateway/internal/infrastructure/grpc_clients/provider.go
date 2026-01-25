package grpc_clients

import (
	"api-gateway/internal/infrastructure/grpc_clients/identity"

	"github.com/google/wire"
)

var ClientSet = wire.NewSet(
	NewCalcGrpcClient,
	identity.NewIdentityClientFactory,
	identity.ProvideAuthClient,
)
