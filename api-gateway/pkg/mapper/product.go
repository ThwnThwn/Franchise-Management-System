package mapper

import (
	"api-gateway/internal/domain/models/product"
	pb "api-gateway/pkg/pb/product"
)

func MapProductPbToProductModel(p *pb.Product) *product.Product {
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
	return &product.Product{
		Id:           p.Id,
		Name:         p.Name,
		Description:  p.Description,
		ImageUrl:     p.ImageUrl,
		IsActive:     p.IsActive,
		CategoryId:   p.CategoryId,
		CategoryName: p.CategoryName,
		Sizes:        sizes,
	}
}
