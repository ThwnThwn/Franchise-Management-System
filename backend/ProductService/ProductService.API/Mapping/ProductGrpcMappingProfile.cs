using AutoMapper;
using ProductService.Application.Features.Products.Queries.GetById;
using ProductService.Application.Features.Products.Queries.GetByProductName;
using ProductService.Application.Features.Products.Queries.GetList;
using ProductService.Domain.Entities;

namespace ProductService.API.Mapping;

public class ProductGrpcMappingProfile : Profile
{
    public ProductGrpcMappingProfile()
    {
        CreateMap<ProductSizeListItemDto, ProductSize>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Sku, opt => opt.MapFrom(s => s.Sku ?? string.Empty))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name ?? string.Empty))
            .ForMember(d => d.Attributes, opt => opt.MapFrom(s => s.Attributes ?? string.Empty))
            .ForMember(d => d.BasePrice, opt => opt.MapFrom(s => (double)s.BasePrice))
            .ForMember(d => d.Cost, opt => opt.MapFrom(s => (double)s.Cost))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive));

        CreateMap<GetProductListItemDto, Product>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name ?? string.Empty))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description ?? string.Empty))
            .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl ?? string.Empty))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive))
            .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId.ToString()))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.CategoryName ?? string.Empty))
            .ForMember(d => d.Sizes, opt => opt.MapFrom(s => s.Sizes));

        CreateMap<ProductSizeDto, ProductSize>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Sku, opt => opt.MapFrom(s => s.Sku ?? string.Empty))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name ?? string.Empty))
            .ForMember(d => d.Attributes, opt => opt.MapFrom(s => s.Attributes ?? string.Empty))
            .ForMember(d => d.Cost, opt => opt.MapFrom(s => (double)s.Cost))
            .ForMember(d => d.BasePrice, opt => opt.MapFrom(s => (double)s.BasePrice))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive));

        CreateMap<GetProductDetailDto, Product>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name ?? string.Empty))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description ?? string.Empty))
            .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl ?? string.Empty))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive))
            .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId.ToString()))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.CategoryName ?? string.Empty))
            .ForMember(d => d.Sizes, opt => opt.MapFrom(s => s.Sizes));


        CreateMap<ProductVariant, ProductSizeDto>()
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true));


        CreateMap<GetProductByNameQueryResponse, Product>()
    .ForMember(d => d.Id, opt => opt.MapFrom(s => s.ProductId.ToString()))
    .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
    .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
    .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl))
    .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive))
    .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt.ToUnixTimeSeconds()))
    .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(
        s => s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToUnixTimeSeconds() : 0));

    }
}
