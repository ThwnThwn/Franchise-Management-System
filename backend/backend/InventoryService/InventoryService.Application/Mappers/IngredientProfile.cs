using AutoMapper;
using InventoryService.Application.Dtos.Ingredient;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Mappers
{
    public class IngredientProfile : Profile
    {
        public IngredientProfile()
        {
            CreateMap<UpdateIngredientReq, Ingredient>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) =>
            {
                if (srcMember == null) return false;

                if (srcMember is string s && string.IsNullOrWhiteSpace(s)) return false;

                return true;
            }));
            CreateMap<Ingredient, IngredientViewRes>();
        }


    }
}
