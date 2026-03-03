package franchise

import (
	"api-gateway/config"
	"api-gateway/pkg/logger"
	"api-gateway/pkg/utils/grpcutils"

	"google.golang.org/grpc"
)

type clientFactory struct {
	conn *grpc.ClientConn
}

func NewFranchiseClientFactory(url config.FranchiseServiceUrl, l logger.Interface) (*clientFactory, func(), error) {
	conn, cleanup, err := grpcutils.NewClientConn(string(url), "Franchise Service", l)
	if err != nil {
		return nil, nil, err
	}
	return &clientFactory{conn: conn}, cleanup, nil
}
