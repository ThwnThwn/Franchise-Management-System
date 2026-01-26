using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ProductService.Application.Features.Categories.GetList;
using ProductService.Domain.Entities;

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
        }
    }
}
