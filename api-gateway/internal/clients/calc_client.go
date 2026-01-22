package clients

import (
	"fmt"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"

	pb "api-gateway/pkg/pb/calc"
)

type CalcServiceClient struct {
	Client pb.CalculatorClient
}

func NewCalcServiceClient(url string) (*CalcServiceClient, error) {
	conn, err := grpc.NewClient(url, grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		return nil, fmt.Errorf("Cannot connect to gRPC server at %s: %w", url, err)
	}

	return &CalcServiceClient{
		Client: pb.NewCalculatorClient(conn),
	}, nil
}
