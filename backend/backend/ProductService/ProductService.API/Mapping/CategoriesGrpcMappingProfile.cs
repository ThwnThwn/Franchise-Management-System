using AutoMapper;
using ProductService.Application.Features.Categories.Command.UpdateCategory;
using ProductService.Application.Features.Categories.Queries.GetByCategoryName;
using ProductService.Application.Features.Categories.Queries.GetList;
using static ProductService.Application.Features.Categories.Command.CreateCategory.CreateCategoryCommand;
using static ProductService.Application.Features.Categories.Command.DeleteCategory.DeleteCategoryCommand;
using static ProductService.Application.Features.Categories.Queries.GetByCategoryName.GetCategoryByNameQuery;
using static ProductService.Application.Features.Categories.Queries.GetById.GetCategoryByIdQuery;


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

        CreateMap<GetCategoryByIdResponse, Category>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description ?? string.Empty))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive));

        CreateMap<CreateCategoryCommandResponse, Category>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description ?? string.Empty))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive));

        CreateMap<UpdateCategoryCommandResponse, Category>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description ?? string.Empty))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive));

        CreateMap<DeleteCategoryCommandResponse, Category>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()));

        CreateMap<GetCategoryByNameQueryResponse, Category>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description ?? string.Empty))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive));

    }
}
