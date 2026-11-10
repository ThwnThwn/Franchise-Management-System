package sales

import (
	"api-gateway/config"
	"api-gateway/pkg/logger"
	"api-gateway/pkg/utils/grpcutils"

	"google.golang.org/grpc"
)

type salesClientFactory struct {
	conn *grpc.ClientConn
}

func NewSalesClientFactory(url config.SalesServiceUrl, cfg *config.Config, l logger.Interface) (*salesClientFactory, func(), error) {
	// conn, cleanup, err := grpcutils.NewClientConn(string(url), "Sales Service", l)
	maxSize := cfg.MaxUploadSizeMb * 1024 * 1024
	conn, cleanup, err := grpcutils.NewClientConn(
		string(url),
		"Sales Service",
		l,
		grpc.WithDefaultCallOptions(
			grpc.MaxCallSendMsgSize(maxSize),
			grpc.MaxCallRecvMsgSize(maxSize),
		),
	)
	if err != nil {
		return nil, nil, err
	}
	return &salesClientFactory{conn: conn}, cleanup, nil
}
