package usecases

import (
	"api-gateway/internal/application/abstractions"
	productGW "api-gateway/internal/application/gateways/product"
	"api-gateway/internal/domain/models/category"
	"context"
)

type CategoryUseCase struct {
	service productGW.ICategoryService
}

func NewCategoryUseCase(service productGW.ICategoryService) abstractions.ICategoryUseCase {
	return &CategoryUseCase{service: service}
}

// GetCategories implements abstractions.ICategoryUseCase.
func (c *CategoryUseCase) GetCategories(ctx context.Context) ([]category.Category, error) {
	return c.service.GetCategories(ctx)
}
