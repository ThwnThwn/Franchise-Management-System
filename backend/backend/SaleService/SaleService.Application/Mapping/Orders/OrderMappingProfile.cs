using AutoMapper;
using SalesService.Application.Features.Orders.Commands.Create;
using SalesService.Domain.Entities;

namespace SalesService.Application.Mapping.Orders;

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
