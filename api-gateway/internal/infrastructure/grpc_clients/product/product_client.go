package product

import (
	"api-gateway/config"
	productGW "api-gateway/internal/application/gateways/product"
	"api-gateway/internal/domain/entity"
	"api-gateway/internal/domain/models/product"
	productPb "api-gateway/pkg/pb/product"
	"context"
	"fmt"

	"github.com/google/uuid"
	"github.com/jinzhu/copier"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

type ProductGrpcClient struct {
	client productPb.ProductGrpcClient
}

func NewProductClient(url config.ProductServiceUrl) (productGW.IProductService, error) {
	conn, err := grpc.NewClient(string(url),
		grpc.WithTransportCredentials(insecure.NewCredentials()),
		grpc.WithDefaultServiceConfig(`{"loadBalancingConfig": [{"round_robin":{}}]}`),
	)
	if err != nil {
		return nil, fmt.Errorf("Cannot connect to gRPC server at %s: %w", url, err)
	}
	return &ProductGrpcClient{
		client: productPb.NewProductGrpcClient(conn),
	}, nil
}

func (c *ProductGrpcClient) GetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error) {
	res, err := c.client.GetProducts(ctx, &productPb.GetProductsRequest{CategoryId: req.CategoryId.String()})
	if err != nil {
		return nil, err
	}

	products := make([]*entity.Product, len(res.Products))
	copier.Copy(products, res.Products)
	return &product.GetProductsReply{
		Products:   products,
		TotalCount: res.TotalCount,
	}, nil
}

func (c *ProductGrpcClient) GetProductById(ctx context.Context, id uuid.UUID) (*entity.Product, error) {
	res, err := c.client.GetProductById(ctx, &productPb.GetProductByIdRequest{Id: id.String()})
	if err != nil {
		return nil, err
	}

	productRes := res.Product
	sizes := make([]*entity.ProductSize, len(productRes.Sizes))
	copier.Copy(sizes, productRes.Sizes)
	return &entity.Product{
		Id:           productRes.Id,
		Name:         productRes.Name,
		Description:  productRes.Description,
		ImageUrl:     productRes.ImageUrl,
		IsActive:     productRes.IsActive,
		CategoryId:   productRes.CategoryId,
		CategoryName: productRes.CategoryName,
		Sizes:        sizes,
	}, nil
}
