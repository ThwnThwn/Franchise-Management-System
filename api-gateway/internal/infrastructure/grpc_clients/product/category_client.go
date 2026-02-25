package product

import (
	productGW "api-gateway/internal/application/gateways/product"
	"context"

	"api-gateway/internal/domain/models/category"
	productPb "api-gateway/pkg/pb/product"
	"api-gateway/pkg/utils/stringutils"

	"github.com/google/uuid"
)

type CategoryGrpcClient struct {
	client productPb.CategoryGrpcClient
}

func ProvideCategoryClient(f *clientFactory) productGW.ICategoryService {
	return &CategoryGrpcClient{client: productPb.NewCategoryGrpcClient(f.conn)}
}

// GetCategories implements product.ICategoryService.
func (c *CategoryGrpcClient) GetCategories(ctx context.Context) ([]*category.Category, error) {
	res, err := c.client.GetCategories(ctx, &productPb.GetCategoriesRequest{})
	if err != nil {
		return nil, err
	}

	categories := make([]*category.Category, 0, len(res.Categories))
	for _, c := range res.Categories {
		categories = append(categories, &category.Category{
			Id:          c.Id,
			Name:        c.Name,
			Description: c.Description,
			ImageUrl:    c.ImageUrl,
			IsActive:    c.IsActive,
		})
	}
	return categories, nil
}

// GetById implements product.ICategoryService.
func (c *CategoryGrpcClient) GetById(ctx context.Context, id uuid.UUID) (*category.Category, error) {
	res, err := c.client.GetCategoryById(ctx, &productPb.GetCategoryByIdRequest{
		Id: id.String(),
	})
	if err != nil {
		return nil, err
	}

	item := res.Category
	return &category.Category{
		Id:          item.Id,
		Name:        item.Name,
		Description: item.Description,
		ImageUrl:    item.ImageUrl,
		IsActive:    item.IsActive,
	}, nil
}

// Create implements product.ICategoryService.
func (c *CategoryGrpcClient) Create(ctx context.Context, req *category.CreateCategoryReq) error {
	_, err := c.client.CreateCategory(ctx, &productPb.CreateCategoryRequest{
		Name:        req.Name,
		Description: req.Description,
	})
	return err
}

// Update implements product.ICategoryService.
func (c *CategoryGrpcClient) Update(ctx context.Context, id uuid.UUID, req *category.UpdateCategoryReq) error {
	_, err := c.client.UpdateCategory(ctx, &productPb.UpdateCategoryRequest{
		Id:          id.String(),
		Name:        stringutils.GetString(req.Name),
		Description: stringutils.GetString(req.Description),
	})
	return err
}

// Delete implements product.ICategoryService.
func (c *CategoryGrpcClient) Delete(ctx context.Context, id uuid.UUID) error {
	_, err := c.client.DeleteCategory(ctx, &productPb.DeleteCategoryRequest{
		Id: id.String(),
	})
	return err
}
