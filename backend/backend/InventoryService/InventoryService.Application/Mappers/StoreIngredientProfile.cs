using AutoMapper;
using InventoryService.Application.Dtos.StoreIngredient;

namespace InventoryService.Application.Mappers
{
    public class StoreIngredientProfile : Profile
    {
        public StoreIngredientProfile()
        {
            CreateMap<Domain.Entities.StoreIngredient, StockViewRes>()
                        .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src =>
                            src.Quantity > src.MinThreshold ? 0 :   // TH1: > MinThreshold
                            src.Quantity == 0 ? 2 :                 // TH2: = 0
                            1                                       // TH3: Các trường hợp còn lại (0 < Quantity <= MinThreshold)
                        ));

        }
    }
}
