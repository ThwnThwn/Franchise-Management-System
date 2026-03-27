using InventoryService.Application.Dtos.StoreIngredient;
using MediatR;

namespace InventoryService.Application.Features.StoreIngredient.Commands.DecreaseStockByOrder
{
    public record class DecreaseStockByOrderCommand(DecreaseStockByOrderReq Request) : IRequest<Unit>;
}
