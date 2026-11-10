package supplier

import (
	"api-gateway/config"
	"api-gateway/pkg/logger"
	"api-gateway/pkg/utils/grpcutils"

	"google.golang.org/grpc"
)

type clientFactory struct {
	conn *grpc.ClientConn
}

func NewSupplierClientFactory(url config.SupplierServiceUrl, l logger.Interface) (*clientFactory, func(), error) {
	conn, cleanup, err := grpcutils.NewClientConn(string(url), "Supplier Service", l)
	if err != nil {
		return nil, nil, err
	}
	return &clientFactory{conn: conn}, cleanup, nil
}
