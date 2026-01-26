package sales

import (
	"api-gateway/config"
	"fmt"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

type salesClientFactory struct {
	conn *grpc.ClientConn
}

func NewSalesClientFactory(url config.SalesServiceUrl) (*salesClientFactory, error) {
	conn, err := grpc.NewClient(string(url),
		grpc.WithTransportCredentials(insecure.NewCredentials()),
		grpc.WithDefaultServiceConfig(`{"loadBalancingConfig": [{"round_robin":{}}]}`),
	)
	if err != nil {
		return nil, fmt.Errorf("Cannot connect to gRPC server at %s: %w", url, err)
	}
	return &salesClientFactory{conn: conn}, nil
}
