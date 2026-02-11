using InventoryService.Application.Dtos.StoreIngredient;
using MediatR;
using System.Collections.Generic;

namespace InventoryService.Application.Features.StoreIngredient.Commands.ReserveStock;

public class ReserveStockCommand : IRequest<CheckEnoughStockRes>
{
    public Guid StoreId { get; set; }
    public Guid OrderId { get; set; }
    public List<AmountVariant> Items { get; set; } = new();
}

public class ReserveStockResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
