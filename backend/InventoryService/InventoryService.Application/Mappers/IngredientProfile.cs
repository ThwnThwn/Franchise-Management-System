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

        // Chỉ map nếu giá trị nguồn:
        // 1. Khác null
        // 2. Nếu là string thì phải khác "" (Empty)
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) =>
        {
            if (srcMember == null) return false;

            if (srcMember is string s && string.IsNullOrWhiteSpace(s)) return false;

            return true;
        }));

        }
    }
}
