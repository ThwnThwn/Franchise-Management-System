package product

import (
	productGW "api-gateway/internal/application/gateways/product"

	"api-gateway/internal/domain/models/product"
	productPb "api-gateway/pkg/pb/product"
	"context"

	"github.com/google/uuid"
	"github.com/jinzhu/copier"
)

type ProductGrpcClient struct {
	client productPb.ProductGrpcClient
}

func ProvideProductClient(f *clientFactory) productGW.IProductService {
	return &ProductGrpcClient{client: productPb.NewProductGrpcClient(f.conn)}
}

func (c *ProductGrpcClient) GetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error) {
	res, err := c.client.GetProducts(ctx, &productPb.GetProductsRequest{CategoryId: req.CategoryId.String()})
	if err != nil {
		return nil, err
	}

	products := make([]*product.Product, len(res.Products))
	for _, p := range res.Products {
		sizes := make([]*product.ProductSize, len(p.Sizes))
		copier.Copy(sizes, p.Sizes)
		products = append(products, &product.Product{
			Id:           p.Id,
			Name:         p.Name,
			Description:  p.Description,
			ImageUrl:     p.ImageUrl,
			IsActive:     p.IsActive,
			CategoryId:   p.CategoryId,
			CategoryName: p.CategoryName,
			Sizes:        sizes,
		})
	}
	return &product.GetProductsReply{
		Products:   products,
		TotalCount: res.TotalCount,
	}, nil
}

func (c *ProductGrpcClient) GetProductById(ctx context.Context, id uuid.UUID) (*product.Product, error) {
	res, err := c.client.GetProductById(ctx, &productPb.GetProductByIdRequest{Id: id.String()})
	if err != nil {
		return nil, err
	}

	productRes := res.Product
	sizes := make([]*product.ProductSize, len(productRes.Sizes))
	copier.Copy(sizes, productRes.Sizes)
	return &product.Product{
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
