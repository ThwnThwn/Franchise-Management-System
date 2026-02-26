using Microsoft.EntityFrameworkCore;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Infrastructure.DbContext;

namespace SupplierService.Infrastructure.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly SupplierDbContext _dbContext;

    public PurchaseOrderRepository(SupplierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PurchaseOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PurchaseOrders
            .Include(po => po.Items)
            .Include(po => po.Supplier)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);
    }

    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        await _dbContext.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
    }

    public async Task<bool> SupplierExistsAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Suppliers
            .AnyAsync(s => s.Id == supplierId && !s.IsDeleted, cancellationToken);
    }

    public Task<string> GenerateNextCodeAsync(CancellationToken cancellationToken = default)
    {
        var code = $"PO-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        return Task.FromResult(code);
    }

    public async Task<Dictionary<Guid, decimal>> GetSupplierIngredientPricesAsync(
        Guid supplierId, IEnumerable<Guid> ingredientIds, CancellationToken cancellationToken = default)
    {
        var idList = ingredientIds.ToList();
        return await _dbContext.SupplierIngredients
            .Where(si => si.SupplierId == supplierId && idList.Contains(si.IngredientId))
            .ToDictionaryAsync(si => si.IngredientId, si => si.UnitPrice, cancellationToken);
    }
    public IQueryable<PurchaseOrder> GetQueryable()
    {
        return _dbContext.PurchaseOrders.AsQueryable();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
