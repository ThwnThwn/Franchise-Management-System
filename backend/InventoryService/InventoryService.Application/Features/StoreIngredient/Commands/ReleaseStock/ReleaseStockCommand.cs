using InventoryService.Application.Dtos.StoreIngredient;
using MediatR;

namespace InventoryService.Application.Features.StoreIngredient.Commands.ReleaseStock
{
    public record ReleaseStockCommand(ReleaseStockReq Request) : IRequest<ReleaseStockRes>;
}
