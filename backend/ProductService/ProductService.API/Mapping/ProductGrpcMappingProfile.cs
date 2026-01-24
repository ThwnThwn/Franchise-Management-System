using AutoMapper;
using ProductService.API;
using ProductService.Application.DTOs.Responses;

namespace ProductService.API.Mapping;

public class ProductGrpcMappingProfile : Profile
{
    public ProductGrpcMappingProfile()
    {
        // Application DTO -> gRPC Messages
        CreateMap<ProductSizeResponse, ProductSize>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Sku, opt => opt.MapFrom(s => s.Sku ?? string.Empty))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name ?? string.Empty))
            .ForMember(d => d.Attributes, opt => opt.MapFrom(s => s.Attributes ?? string.Empty))
            .ForMember(d => d.BasePrice, opt => opt.MapFrom(s => (double)s.BasePrice))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive));

        CreateMap<ProductResponse, Product>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name ?? string.Empty))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description ?? string.Empty))
            .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl ?? string.Empty))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive))
            .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId.ToString()))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.CategoryName ?? string.Empty))
            .ForMember(d => d.Sizes, opt => opt.MapFrom(s => s.Sizes));
    }
}
