using SalesService.Domain.Entities;

namespace SalesService.Application.Interfaces.Payments;

public interface IPaymentRepository
{
    Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default);

    Task<List<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<Payment?> GetByIdWithOrderAsync(Guid paymentId, CancellationToken cancellationToken = default);

    Task AddEventAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

