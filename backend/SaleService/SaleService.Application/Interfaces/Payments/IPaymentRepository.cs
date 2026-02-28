using BuildingBlocks.Core.Pagination;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Interfaces.Payments;

public interface IPaymentRepository
{
    Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default);

    Task<List<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Payment?> GetNewestByOrderIdAndStatusAsync( Guid orderId,PaymentStatus status,CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdWithOrderAsync(Guid paymentId, CancellationToken cancellationToken = default);

    Task AddEventAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<Payment>> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Payment>> GetMyPaymentHistoryAsync(
        Guid customerId,
        int page,
        int pageSize,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? status = null,
        CancellationToken cancellationToken = default);
}

