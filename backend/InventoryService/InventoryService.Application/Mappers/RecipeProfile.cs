using AutoMapper;
using InventoryService.Application.Dtos.Recipe;
using InventoryService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Mappers
{
    public class RecipeProfile : Profile
    {
        public RecipeProfile()
        {
            CreateMap<Recipe, RecipeViewRes>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Ingredient.Name))
                .ForMember(dest => dest.BaseUnit, opt => opt.MapFrom(src => src.Ingredient.BaseUnit));
        }
    }
}
