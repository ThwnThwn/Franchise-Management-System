using SupplierService.Domain.Entities;

namespace SupplierService.Application.Repositories;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);
    Task<bool> SupplierExistsAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<string> GenerateNextCodeAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, decimal>> GetSupplierIngredientPricesAsync(Guid supplierId, IEnumerable<Guid> ingredientIds, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, decimal>> GetLatestPriceForIngredientsAsync(IEnumerable<Guid> ingredientIds, CancellationToken cancellationToken = default);
    IQueryable<PurchaseOrder> GetQueryable();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
