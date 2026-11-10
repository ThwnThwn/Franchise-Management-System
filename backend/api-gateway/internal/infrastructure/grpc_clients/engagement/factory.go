package engagement

import (
	"api-gateway/config"
	"api-gateway/pkg/logger"
	"api-gateway/pkg/utils/grpcutils"

	"google.golang.org/grpc"
)

type clientFactory struct {
	conn *grpc.ClientConn
}

func NewEngagementClientFactory(url config.EngagementServiceUrl, l logger.Interface) (*clientFactory, func(), error) {
	conn, cleanup, err := grpcutils.NewClientConn(string(url), "Engagement Service", l)
	if err != nil {
		return nil, nil, err
	}
	return &clientFactory{conn: conn}, cleanup, nil
}
