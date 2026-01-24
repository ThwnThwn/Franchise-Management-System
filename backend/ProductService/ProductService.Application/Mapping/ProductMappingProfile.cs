using AutoMapper;
using ProductService.Application.DTOs.Responses;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Domain -> Application DTOs
        CreateMap<ProductVariant, ProductSizeResponse>()
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true));

        CreateMap<Product, ProductResponse>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : ""))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true))
            .ForMember(d => d.Sizes, opt => opt.MapFrom(s => s.ProductVariants));
    }
}

