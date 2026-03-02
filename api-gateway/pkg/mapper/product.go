package mapper

import (
	"api-gateway/internal/domain/models/product"
	pb "api-gateway/pkg/pb/product"
)

func MapProductPbToProductModel(p *pb.Product) *product.Product {
	variants := make([]*product.ProductVariant, 0, len(p.Variants))
	for _, s := range p.Variants {
		variants = append(variants, &product.ProductVariant{
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
		Id:           p.Id,
		Name:         p.Name,
		Description:  p.Description,
		ImageUrl:     p.ImageUrl,
		IsActive:     p.IsActive,
		CategoryId:   p.CategoryId,
		CategoryName: p.CategoryName,
		Variants:     variants,
	}
}
