package product

import (
	"api-gateway/config"
	"api-gateway/pkg/logger"
	"api-gateway/pkg/utils/grpcutils"

	"google.golang.org/grpc"
)

type clientFactory struct {
	conn *grpc.ClientConn
}

func NewProductClientFactory(url config.ProductServiceUrl, cfg *config.Config, l logger.Interface) (*clientFactory, func(), error) {
	maxSize := cfg.MaxUploadSizeMb * 1024 * 1024

	conn, cleanup, err := grpcutils.NewClientConn(
		string(url),
		"Product Service",
		l,
		grpc.WithDefaultCallOptions(
			grpc.MaxCallSendMsgSize(maxSize),
			grpc.MaxCallRecvMsgSize(maxSize),
		),
	)

	if err != nil {
		return nil, nil, err
	}
	return &clientFactory{conn: conn}, cleanup, nil
}
