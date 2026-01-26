using System.Collections.Generic;
using AutoMapper;
using ProductService.API;
using ProductService.Application.Features.Categories.GetList;

namespace ProductService.API.Mapping;

public class CategoriesGrpcMappingProfile : Profile
{
    public CategoriesGrpcMappingProfile()
    {
        CreateMap<GetCategoryListItemDto, Category>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name ?? string.Empty))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description ?? string.Empty))
            .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl ?? string.Empty))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive));
    }
}
