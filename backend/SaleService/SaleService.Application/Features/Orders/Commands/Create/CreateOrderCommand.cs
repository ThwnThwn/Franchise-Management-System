using Ardalis.Result;
using MediatR;

namespace SalesService.Application.Features.Orders.Commands.Create;

public sealed record CreateOrderCommand(CreateOrderRequestDto Request)
    : IRequest<Result<CreateOrderResponseDto>>;

public sealed class CreateOrderRequestDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal? Tax { get; set; }
    public int? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? StoreId { get; set; }
}

public sealed class CartItemDto
{
    public string Sku { get; set; } = default!;
    public int Quantity { get; set; }
}

public sealed class CreateOrderResponseDto
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = default!;
    public decimal TotalAmount { get; set; }
}
