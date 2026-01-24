using AutoMapper;
using ProductService.Application.DTOs.Responses;
using ProductService.Application.Features.Products.GetById;
using ProductService.Application.Features.Products.GetList;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Mapping cho DTO cũ (REST + gRPC đang dùng)
        CreateMap<ProductVariant, ProductSizeResponse>()
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true));

        CreateMap<Product, ProductResponse>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : ""))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true))
            .ForMember(d => d.Sizes, opt => opt.MapFrom(s => s.ProductVariants));

        // Mapping cho CQRS Features - GetById
        CreateMap<ProductVariant, ProductSizeDto>()
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true));

        CreateMap<Product, GetProductDetailDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : ""))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true))
            .ForMember(d => d.Sizes, opt => opt.MapFrom(s => s.ProductVariants));

        // Mapping cho CQRS Features - GetList
        CreateMap<ProductVariant, ProductSizeListItemDto>()
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true));

        CreateMap<Product, GetProductListItemDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : ""))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true))
            .ForMember(d => d.Sizes, opt => opt.MapFrom(s => s.ProductVariants));
    }
}

