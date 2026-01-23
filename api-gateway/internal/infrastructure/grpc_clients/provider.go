package grpc_clients

import "github.com/google/wire"

var ClientSet = wire.NewSet(
	NewCalcGrpcClient,
)
