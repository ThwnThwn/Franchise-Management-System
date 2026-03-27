using ProductService.Application.Dtos;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mapping;

public static class ProductMappingExtentions
{
    public static ProductDto MapToProductDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            IsDeleted = product.IsDeleted,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,

            Variants = (product.ProductVariants ?? []).Select(v => new ProductVariantDto
            {
                Id = v.Id,
                Name = v.Name,
                Attributes = v.Attributes,
                BasePrice = v.BasePrice,
                Cost = v.Cost,
                IsActive = v.IsActive,
                Sku = v.Sku
            }).ToList()
        };
    }
}