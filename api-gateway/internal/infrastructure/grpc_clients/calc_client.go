package grpc_clients

import (
	"context"
	"fmt"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"

	"api-gateway/internal/application/gateways"
	pb "api-gateway/pkg/pb/calc"
)

type CalcGrpcClient struct {
	client pb.CalculatorClient
}

func NewCalcGrpcClient(url string) (gateways.ICalcService, error) {
	conn, err := grpc.NewClient(url, grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		return nil, fmt.Errorf("Cannot connect to gRPC server at %s: %w", url, err)
	}

	return &CalcGrpcClient{
		client: pb.NewCalculatorClient(conn),
	}, nil
}

func (c *CalcGrpcClient) Add(ctx context.Context, a int32, b int32) (int32, error) {
	resp, err := c.client.Add(ctx, &pb.AddRequest{NumberA: a, NumberB: b})
	if err != nil {
		return 0, err
	}
	return resp.Result, nil
}
