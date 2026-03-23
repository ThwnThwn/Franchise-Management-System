namespace WarehouseService.Application.ClientInterfaces;

public interface IInventoryServiceClient
{
    Task<bool> IngredientExistsAsync(Guid ingredientId);
}