using SupplierService.Domain.Entities;

namespace SupplierService.Application.Repositories;

public interface ISupplierPaymentRepository
{
    Task<SupplierPayment> CreateAsync(SupplierPayment payment, CancellationToken cancellationToken = default);

    Task<SupplierPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<SupplierPayment>> GetByPurchaseOrderIdAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default);

    Task<(List<SupplierPayment> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        Guid? supplierId = null,
        Guid? purchaseOrderId = null,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

