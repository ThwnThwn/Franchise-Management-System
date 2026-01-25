using AutoMapper;
using SaleService.Application.Features.Orders.Create;
using SaleService.Domain.Entities;

namespace SaleService.Application.Mapping.Orders;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, CreateOrderResponseDto>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount));
    }
}
