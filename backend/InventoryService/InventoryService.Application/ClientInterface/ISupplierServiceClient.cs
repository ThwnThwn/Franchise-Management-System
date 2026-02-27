namespace InventoryService.Application.ClientInterface
{
    public interface ISupplierServiceClient
    {
        Task<Dictionary<Guid, decimal>> GetLatestPriceForIngredientsAsync(IEnumerable<Guid> ingredientIds, CancellationToken cancellationToken = default);
    }
}
