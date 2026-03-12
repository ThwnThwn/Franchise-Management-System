using Ardalis.Result;
using MediatR;
using SalesService.Application.Dtos.Inventory;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Orders.Commands.Create;

public sealed record CreateOrderCommand(CreateOrderRequestDto Request)
    : IRequest<Result<CreateOrderResponseDto>>;

public sealed class CreateOrderRequestDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? StoreId { get; set; }
    public OrderType? Type { get; set; }
    public TimeOnly? ScheduledTime { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
}

public sealed class CartItemDto
{
    public string Sku { get; set; } = default!;
    public int Quantity { get; set; }

}

public sealed class CreateOrderResponseDto
{
    public Guid? OrderId { get; set; }
    public string? OrderCode { get; set; } = default!;
    public List<OrderItemDto> Items { get; set; } = new();

    public Guid? StoreId { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public CheckEnoughStockRes CheckEnoughStockRes { get; set; } = new();
    public OrderType? Type { get; set; }
    public TimeOnly? ScheduledTime { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public decimal? Distance { get; set; }
}
public sealed class OrderItemDto
{
    public string Sku { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

}



