using Microsoft.EntityFrameworkCore;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Infrastructure.DbContext;

namespace SupplierService.Infrastructure.Repositories;

public class SupplierPaymentRepository : ISupplierPaymentRepository
{
    private readonly SupplierDbContext _dbContext;

    public SupplierPaymentRepository(SupplierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SupplierPayment> CreateAsync(SupplierPayment payment, CancellationToken cancellationToken = default)
    {
        await _dbContext.SupplierPayments.AddAsync(payment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<SupplierPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SupplierPayments
            .Include(p => p.PurchaseOrder)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<SupplierPayment>> GetByPurchaseOrderIdAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SupplierPayments
            .Where(p => p.PurchaseOrderId == purchaseOrderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<SupplierPayment> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        Guid? supplierId = null,
        Guid? purchaseOrderId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SupplierPayments
            .Include(p => p.PurchaseOrder)
            .AsQueryable();

        if (purchaseOrderId.HasValue)
            query = query.Where(p => p.PurchaseOrderId == purchaseOrderId.Value);

        if (supplierId.HasValue)
            query = query.Where(p => p.PurchaseOrder.SupplierId == supplierId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
