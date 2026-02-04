package product

import (
	"api-gateway/internal/domain/models/common"
)

type GetProductsRequest struct {
	CategoryId string `form:"categoryId"`
}

type GetProductsByKeywordRequest struct {
	common.PaginationQuery
	Keyword    *string `form:"keyword"`
	CategoryId *string `form:"categoryId"`
}

type CreateProductRequest struct {
	Name        string  `json:"name" binding:"required"`
	Description *string `json:"description"`
	ImageUrl    *string `json:"imaageUrl"`
	// ImagePublicId *string                 `json:"imagePublicId"`
	CategoryId string                  `json:"categoryId"`
	Variants   []*CreateProductVariant `json:"variants"`
}

type CreateProductVariant struct {
	Sku        *string `json:"sku"`
	Name       *string `json:"name"`
	Attributes *string `json:"attributes"`
	BasePrice  float64 `json:"basePrice"`
	Cost       float64 `json:"cost"`
	IsActive   *bool   `json:"isActive"`
}

type UpdateProductRequest struct {
	Name        *string `json:"name"`
	Description *string `json:"description"`
	ImageUrl    *string `json:"imageUrl"`
	// ImagePublicId *string                 `json:"ImagePublicId"`
	CategoryId *string                 `json:"categoryId"`
	Variants   []*UpdateProductVariant `json:"variants"`
	IsActive   *bool                   `json:"isActive"`
}

type UpdateProductVariant struct {
	Sku        *string  `json:"sku"`
	Name       *string  `json:"name"`
	Attributes *string  `json:"attributes"`
	BasePrice  *float64 `json:"basePrice"`
	Cost       *float64 `json:"cost"`
	IsActive   *bool    `json:"isActive"`
}
