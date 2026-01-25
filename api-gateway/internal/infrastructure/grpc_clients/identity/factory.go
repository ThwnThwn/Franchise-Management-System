package identity

import (
	"api-gateway/config"
	"fmt"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

type identityClientFactory struct {
	conn *grpc.ClientConn
}

func NewIdentityClientFactory(url config.IdentityServiceUrl) (*identityClientFactory, error) {
	conn, err := grpc.NewClient(string(url),
		grpc.WithTransportCredentials(insecure.NewCredentials()),
		grpc.WithDefaultServiceConfig(`{"loadBalancingConfig": [{"round_robin":{}}]}`),
	)
	if err != nil {
		return nil, fmt.Errorf("Cannot connect to gRPC server at %s: %w", url, err)
	}
	return &identityClientFactory{conn: conn}, nil
}
