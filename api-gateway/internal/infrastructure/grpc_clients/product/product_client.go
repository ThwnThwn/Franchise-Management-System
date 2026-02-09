package product

import (
	"api-gateway/internal/application/constants/errorcode"
	productGW "api-gateway/internal/application/gateways/product"

	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/product"
	"api-gateway/pkg/mapper"
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

	prods := make([]*product.Product, 0, len(res.Products))
	for _, p := range res.Products {
		prods = append(prods, mapper.MapProductPbToProductModel(p))
	}
	return &product.GetProductsReply{
		Products:   prods,
		TotalCount: res.TotalCount,
	}, nil
}

// GetAllWithPagingBy implements product.IProductService.
func (c *ProductGrpcClient) GetAllWithPagingBy(ctx context.Context, req *product.GetProductsByKeywordRequest) (*common.PaginationResult[*product.Product], error) {
	res, err := c.client.GetProductsByKeyword(ctx, &productPb.GetProductsByKeywordRequest{
		Keyword:    req.Keyword,
		CategoryId: req.CategoryId,
		Page:       req.Page,
		PageSize:   req.PageSize,
	})
	if err != nil {
		return nil, err
	}

	prods := make([]*product.Product, 0, len(res.Products))
	for _, p := range res.Products {
		prods = append(prods, mapper.MapProductPbToProductModel(p))
	}
	return &common.PaginationResult[*product.Product]{
		Items:      prods,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}

func (c *ProductGrpcClient) GetProductById(ctx context.Context, id uuid.UUID) (*product.Product, error) {
	res, err := c.client.GetProductById(ctx, &productPb.GetProductByIdRequest{Id: id.String()})
	if err != nil {
		return nil, err
	}

	return mapper.MapProductPbToProductModel(res.Product), nil
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

// UploadImage implements product.IProductService.
func (c *ProductGrpcClient) UploadImage(ctx context.Context, id uuid.UUID, req *common.UploadImageRequest) (*string, error) {
	if len(req.ImageData) == 0 {
		return nil, errorcode.ErrImageRequired
	}

	res, err := c.client.UploadProductImage(ctx, &productPb.UploadProductImageRequest{
		ProductId:   id.String(),
		ImageData:   req.ImageData,
		FileName:    req.FileName,
		ContentType: req.ContentType,
	})
	if err != nil {
		return nil, err
	}

	return &res.ImageUrl, nil
}
