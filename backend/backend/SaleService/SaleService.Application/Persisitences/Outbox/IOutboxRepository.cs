using SalesService.Domain.Entities;
using SharedKernel.Abstractions;

namespace SalesService.Application.Persisitences.Outbox; // Updated namespace

/// <summary>
/// Repository Outbox cho SaleService.
/// Kế thừa SharedKernel để dùng được extension method AddEvent().
/// </summary>
public interface IOutboxRepository : IOutboxRepository<OutboxMessage>
{
    // Kế thừa void Add(OutboxMessage message) từ SharedKernel
}
