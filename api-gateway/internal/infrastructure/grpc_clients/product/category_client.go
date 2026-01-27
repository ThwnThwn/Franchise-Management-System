package product

import (
	productGW "api-gateway/internal/application/gateways/product"
	"context"

	"api-gateway/internal/domain/models/category"
	productPb "api-gateway/pkg/pb/product"
)

type CategoryGrpcClient struct {
	client productPb.CategoryGrpcClient
}

func ProvideCategoryClient(f *clientFactory) productGW.ICategoryService {
	return &CategoryGrpcClient{client: productPb.NewCategoryGrpcClient(f.conn)}
}

// GetCategories implements product.ICategoryService.
func (c *CategoryGrpcClient) GetCategories(ctx context.Context) ([]category.Category, error) {
	res, err := c.client.GetCategories(ctx, &productPb.GetCategoriesRequest{})
	if err != nil {
		return nil, err
	}

	categories := make([]category.Category, 0, len(res.Categories))
	for _, c := range res.Categories {
		categories = append(categories, category.Category{
			Id:          c.Id,
			Name:        c.Name,
			Description: c.Description,
			ImageUrl:    c.ImageUrl,
			IsActive:    c.IsActive,
		})
	}
	return categories, nil
}
