package grpc_clients

import (
	"api-gateway/config"
	"api-gateway/internal/application/gateways"
	calcPb "api-gateway/pkg/pb/calc"
	"context"
	"fmt"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

type CalcGrpcClient struct {
	client calcPb.CalculatorClient
}

func NewCalcGrpcClient(url config.CalcServiceUrl) (gateways.ICalcService, error) {
	conn, err := grpc.NewClient(string(url),
		grpc.WithTransportCredentials(insecure.NewCredentials()),
		grpc.WithDefaultServiceConfig(`{"loadBalancingConfig": [{"round_robin":{}}]}`),
	)
	if err != nil {
		return nil, fmt.Errorf("Cannot connect to gRPC server at %s: %w", url, err)
	}

	return &CalcGrpcClient{
		client: calcPb.NewCalculatorClient(conn),
	}, nil
}

func (c *CalcGrpcClient) Add(ctx context.Context, a int32, b int32) (int32, error) {
	resp, err := c.client.Add(ctx, &calcPb.AddRequest{NumberA: a, NumberB: b})
	if err != nil {
		return 0, err
	}
	return resp.Result, nil
}
