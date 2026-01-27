package sales

import (
	"api-gateway/config"
	"crypto/tls"
	"fmt"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials"
)

type salesClientFactory struct {
	conn *grpc.ClientConn
}

func NewSalesClientFactory(url config.SalesServiceUrl) (*salesClientFactory, error) {
	// 1. Tạo config TLS chấp nhận mọi chứng chỉ (kể cả chứng chỉ đểu/tự ký)
	tlsConfig := &tls.Config{
		InsecureSkipVerify: true,
	}

	// 2. Tạo credentials từ config trên
	creds := credentials.NewTLS(tlsConfig)

	conn, err := grpc.NewClient(string(url),
		grpc.WithTransportCredentials(creds),
		grpc.WithDefaultServiceConfig(`{"loadBalancingConfig": [{"round_robin":{}}]}`),
	)
	if err != nil {
		return nil, fmt.Errorf("Cannot connect to gRPC server at %s: %w", url, err)
	}
	return &salesClientFactory{conn: conn}, nil
}
