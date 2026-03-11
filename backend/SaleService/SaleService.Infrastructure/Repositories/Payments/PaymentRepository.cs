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
    /// <summary>
    /// Creates the asynchronous.
    /// </summary>
    /// <param name="payment">The payment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _db.Payments.AddAsync(payment, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return payment;
    }
    /// <summary>
    /// Gets the by order identifier asynchronous.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<List<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _db.Payments
            .Where(p => p.OrderId == orderId && (p.IsDeleted == null || p.IsDeleted == false))
            .ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Gets the by identifier with order asynchronous.
    /// </summary>
    /// <param name="paymentId">The payment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<Payment?> GetByIdWithOrderAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _db.Payments
            .Include(p => p.Order)
                .ThenInclude(o => o.OrderItems)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
    }
    /// <summary>
    /// Gets the newest by order identifier and status asynchronous.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="status">The status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<Payment?> GetNewestByOrderIdAndStatusAsync(Guid orderId, PaymentStatus status,CancellationToken cancellationToken = default)
    {
        return await _db.Payments
            .Where(p => p.OrderId == orderId && p.Status == (int)status)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
    /// <summary>
    /// Adds the event asynchronous.
    /// </summary>
    /// <param name="paymentEvent">The payment event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task AddEventAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken = default)
    {
        await _db.PaymentEvents.AddAsync(paymentEvent, cancellationToken);
    }
    /// <summary>
    /// Saves the changes asynchronous.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
    /// <summary>
    /// Gets all asynchronous.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
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
    /// <summary>
    /// Gets the by identifier asynchronous.
    /// </summary>
    /// <param name="paymentId">The payment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken)
    {
       return await _db.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
    }
    /// <summary>
    /// Gets the by reference identifier asynchronous.
    /// </summary>
    /// <param name="paymentId">The payment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<Payment?> GetByReferenceIdAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        return await _db.Payments
             .FirstOrDefaultAsync(p => p.ReferenceId == paymentId, cancellationToken);
    }
    /// <summary>
    /// Updates the asynchronous.
    /// </summary>
    /// <param name="payment">The payment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Không tìm thấy {typeof(Payment).Name}</exception>
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
    /// <summary>
    /// Gets my payment history asynchronous.
    /// </summary>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="page">The page.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="status">The status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<PagedResult<Payment>> GetMyPaymentHistoryAsync(
        Guid customerId,
        int page,
        int pageSize,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Payments
            .Include(p => p.Order)
            .Where(p => (p.IsDeleted == null || p.IsDeleted == false)
                     && p.Order.CustomerId == customerId);

        if (startDate.HasValue)
            query = query.Where(p => p.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.CreatedAt <= endDate.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        query = query.OrderByDescending(p => p.CreatedAt);

        return await query.ToPagedListAsync(page, pageSize, cancellationToken);
    }
}

