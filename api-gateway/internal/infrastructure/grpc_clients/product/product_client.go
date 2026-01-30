package product

import (
	productGW "api-gateway/internal/application/gateways/product"

	"api-gateway/internal/domain/models/product"
	productPb "api-gateway/pkg/pb/product"
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
				Cost:       s.Cost,
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
		TotalCount: res.TotalCount,
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
			Cost:       s.Cost,
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

// CreateProduct implements product.IProductService.
func (c *ProductGrpcClient) CreateProduct(ctx context.Context, req *product.CreateProductRequest) error {
	variants := make([]*productPb.CreateProductVariant, 0, len(req.Variants))
	for _, v := range req.Variants {
		variants = append(variants, &productPb.CreateProductVariant{
			Sku:        v.Sku,
			Name:       v.Name,
			Attributes: v.Attributes,
			BasePrice:  v.BasePrice,
			Cost:       v.Cost,
			IsActive:   v.IsActive,
		})
	}

	_, err := c.client.CreateProduct(ctx, &productPb.CreateProductRequest{
		Name:        req.Name,
		Description: req.Description,
		// ImageUrl: req.ImageUrl,
		CategoryId: req.CategoryId,
		Variants:   variants,
	})
	return err
}

// UpdateProduct implements product.IProductService.
func (c *ProductGrpcClient) UpdateProduct(ctx context.Context, id uuid.UUID, req *product.UpdateProductRequest) error {
	variants := make([]*productPb.UpdateProductVariant, 0, len(req.Variants))
	for _, v := range req.Variants {
		variants = append(variants, &productPb.UpdateProductVariant{
			Sku:        v.Sku,
			Name:       v.Name,
			Attributes: v.Attributes,
			BasePrice:  v.BasePrice,
			Cost:       v.Cost,
			IsActive:   v.IsActive,
		})
	}

	_, err := c.client.UpdateProduct(ctx, &productPb.UpdateProductRequest{
		ProductId:   id.String(),
		Name:        req.Name,
		Description: req.Description,
		IsActive:    req.IsActive,
		CategoryId:  req.CategoryId,
		Variants:    variants,
	})
	return err
}

// DeleteProduct implements product.IProductService.
func (c *ProductGrpcClient) DeleteProduct(ctx context.Context, id uuid.UUID) error {
	res, err := c.client.DeleteProduct(ctx, &productPb.DeleteProductRequest{Id: id.String()})
	if err != nil {
		return err
	}
	if !res.Success {
		return err
	}
	return nil
}
