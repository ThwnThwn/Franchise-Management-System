using InventoryService.Application.Dtos.StoreIngredient;
using MediatR;

namespace InventoryService.Application.Features.StoreIngredient.Commands.CheckEnoughStock
{
    public record class CheckEnoughStockCommand(CheckEnoughStockReq Request) : IRequest<CheckEnoughStockRes>;
}
