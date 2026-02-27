using MediatR;

namespace InventoryService.Application.Features.StoreIngredient.Commands.SyncZeroCostIngredients
{
    public class SyncZeroCostIngredientsCommand : IRequest<SyncZeroCostIngredientsResult>
    {
    }

    public class SyncZeroCostIngredientsResult
    {
        public int UpdatedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
