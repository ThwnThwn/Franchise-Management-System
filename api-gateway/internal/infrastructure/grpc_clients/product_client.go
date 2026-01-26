package grpc_clients

import (
	"api-gateway/config"
	"api-gateway/internal/application/gateways"
	"api-gateway/pkg/pb/product"
	"context"
	"fmt"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

type ProductGrpcClient struct {
	client product.ProductGrpcClient
}

func NewProductClient(url config.ProductServiceUrl) (gateways.IProductService, error) {
	conn, err := grpc.NewClient(string(url),
		grpc.WithTransportCredentials(insecure.NewCredentials()),
		grpc.WithDefaultServiceConfig(`{"loadBalancingConfig": [{"round_robin":{}}]}`),
	)
	if err != nil {
		return nil, fmt.Errorf("Cannot connect to gRPC server at %s: %w", url, err)
	}
	return &ProductGrpcClient{
		client: product.NewProductGrpcClient(conn),
	}, nil
}

func (c *ProductGrpcClient) GetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error) {
	resp, err := c.client.GetProducts(ctx, req)
	if err != nil {
		return nil, err
	}
	return resp, nil
}

func (c *ProductGrpcClient) GetProductById(ctx context.Context, req *product.GetProductByIdRequest) (*product.GetProductByIdReply, error) {
	resp, err := c.client.GetProductById(ctx, req)
	if err != nil {
		return nil, err
	}
	return resp, nil
}
