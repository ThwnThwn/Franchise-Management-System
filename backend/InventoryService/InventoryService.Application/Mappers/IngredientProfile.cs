using AutoMapper;
using InventoryService.Application.Dtos.Ingredient;
using InventoryService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Mappers
{
    public class IngredientProfile : Profile
    {
        public IngredientProfile() {
            CreateMap<UpdateIngredientReq, Ingredient>()
            // Bỏ qua Map Id vì Id của Entity thường không được thay đổi từ Req
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            // Tự động cập nhật thời gian mỗi khi Map
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTimeOffset.UtcNow))
            // Chỉ map những thuộc tính từ Req có giá trị (khác null)
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}
