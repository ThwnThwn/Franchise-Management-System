using Microsoft.EntityFrameworkCore;
using SalesService.Application.Interfaces.Payments;
using SalesService.Domain.Entities;
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
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
    }

    public async Task AddEventAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken = default)
    {
        await _db.PaymentEvents.AddAsync(paymentEvent, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}

