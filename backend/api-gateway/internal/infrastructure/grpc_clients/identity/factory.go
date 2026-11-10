package identity

import (
	"api-gateway/config"
	"api-gateway/pkg/logger"
	"api-gateway/pkg/utils/grpcutils"

	"google.golang.org/grpc"
)

type identityClientFactory struct {
	conn *grpc.ClientConn
}

func NewIdentityClientFactory(url config.IdentityServiceUrl, l logger.Interface) (*identityClientFactory, func(), error) {
	conn, cleanup, err := grpcutils.NewClientConn(string(url), "Identity Service", l)
	if err != nil {
		return nil, nil, err
	}
	return &identityClientFactory{conn: conn}, cleanup, nil
}
