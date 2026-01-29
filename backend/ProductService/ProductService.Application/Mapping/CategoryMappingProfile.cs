using AutoMapper;
using ProductService.Application.Features.Categories.Command.CreateCategory;
using ProductService.Application.Features.Categories.Command.UpdateCategory;
using ProductService.Application.Features.Categories.Queries.GetList;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.Categories.Command.CreateCategory.CreateCategoryCommand;
using static ProductService.Application.Features.Categories.Command.DeleteCategory.DeleteCategoryCommand;
using static ProductService.Application.Features.Categories.Queries.GetByCategoryName.GetCategoryByNameQuery;
using static ProductService.Application.Features.Categories.Queries.GetById.GetCategoryByIdQuery;

namespace ProductService.Application.Mapping
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<Category, GetCategoryListItemDto>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
                .ForMember(d => d.ImageUrl, opt => opt.Ignore())
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true))
                ;
            CreateMap<Category, GetCategoryByIdResponse>();

            CreateMap<Category, GetCategoryByNameQueryResponse>();

            CreateMap<CreateCategoryCommandRequest, Category>()
    .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
    .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
    .ForMember(d => d.IsActive, opt => opt.MapFrom(_ => true));

            CreateMap<Category, CreateCategoryCommandResponse>();

            CreateMap<Category, UpdateCategoryCommandResponse>();

            CreateMap<Category, DeleteCategoryCommandResponse>()
                .ForMember(d => d.IsDeleted, opt => opt.MapFrom(_ => true))
                .ForMember(d => d.DeletedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));

            CreateMap<UpdateCategoryCommandRequest, Category>()
    .ForMember(d => d.Id, opt => opt.Ignore())
    .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
    .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
    .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive))
    .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));

        }
    }
}
