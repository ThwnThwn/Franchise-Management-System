using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Interfaces.Payments;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.ApplicationDbContext;

namespace SalesService.Infrastructure.Repositories.Payments;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly ISalesDbContext _db;

    public PaymentRepository(ISalesDbContext db)
    {
        _db = db;
    }

    public async Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _db.Payments.AddAsync(payment, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<List<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _db.Payments
            .Where(p => p.OrderId == orderId && (p.IsDeleted == null || p.IsDeleted == false))
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetByIdWithOrderAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _db.Payments
            .Include(p => p.Order)
                .ThenInclude(o => o.OrderItems)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
    }
    public async Task<Payment?> GetNewestByOrderIdAndStatusAsync(Guid orderId, PaymentStatus status,CancellationToken cancellationToken = default)
    {
        return await _db.Payments
            .Where(p => p.OrderId == orderId && p.Status == (int)status)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task AddEventAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken = default)
    {
        await _db.PaymentEvents.AddAsync(paymentEvent, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<Payment>> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Payments
            .Where(p => p.IsDeleted == null || p.IsDeleted == false)
            .OrderByDescending(p => p.CreatedAt);

        return await query.ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public async Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken)
    {
       return await _db.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
    }

    public async Task<Payment?> GetByReferenceIdAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        return await _db.Payments
             .FirstOrDefaultAsync(p => p.ReferenceId == paymentId, cancellationToken);
    }

    public async Task<int> UpdateAsync(Payment payment,
           CancellationToken cancellationToken)
    {
        var entityFromDb = await GetByIdAsync(payment.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy {typeof(Payment).Name}");

        _db.Entry(entityFromDb).CurrentValues.SetValues(payment);

        if (_db.Entry(entityFromDb).Metadata.FindProperty("CreatedAt") != null)
        {
            _db.Entry(entityFromDb).Property("CreatedAt").IsModified = false;
        }

        return await _db.SaveChangesAsync();
    }
}

