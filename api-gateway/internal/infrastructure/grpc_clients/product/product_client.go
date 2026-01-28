package product

import (
	productGW "api-gateway/internal/application/gateways/product"

	"api-gateway/internal/domain/models/product"
	productPb "api-gateway/pkg/pb/product"
	"api-gateway/pkg/utils/numberutils"
	"context"

	"github.com/google/uuid"
)

type ProductGrpcClient struct {
	client productPb.ProductGrpcClient
}

func ProvideProductClient(f *clientFactory) productGW.IProductService {
	return &ProductGrpcClient{client: productPb.NewProductGrpcClient(f.conn)}
}

func (c *ProductGrpcClient) GetProducts(ctx context.Context, req *product.GetProductsRequest) (*product.GetProductsReply, error) {
	res, err := c.client.GetProducts(ctx, &productPb.GetProductsRequest{CategoryId: req.CategoryId})
	if err != nil {
		return nil, err
	}

	products := make([]*product.Product, 0, len(res.Products))
	for _, p := range res.Products {
		sizes := make([]*product.ProductSize, 0, len(p.Sizes))
		for _, s := range p.Sizes {
			sizes = append(sizes, &product.ProductSize{
				Id:         s.Id,
				Sku:        s.Sku,
				Name:       s.Name,
				Attributes: s.Attributes,
				BasePrice:  s.BasePrice,
				IsActive:   s.IsActive,
			})
		}
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
		TotalCount: numberutils.GetInt32(res.TotalCount),
	}, nil
}

func (c *ProductGrpcClient) GetProductById(ctx context.Context, id uuid.UUID) (*product.Product, error) {
	res, err := c.client.GetProductById(ctx, &productPb.GetProductByIdRequest{Id: id.String()})
	if err != nil {
		return nil, err
	}

	productRes := res.Product
	sizes := make([]*product.ProductSize, 0, len(productRes.Sizes))
	for _, s := range productRes.Sizes {
		sizes = append(sizes, &product.ProductSize{
			Id:         s.Id,
			Sku:        s.Sku,
			Name:       s.Name,
			Attributes: s.Attributes,
			BasePrice:  s.BasePrice,
			IsActive:   s.IsActive,
		})
	}
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
