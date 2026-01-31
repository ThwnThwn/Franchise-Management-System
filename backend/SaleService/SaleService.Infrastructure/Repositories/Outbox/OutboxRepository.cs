using SalesService.Application.Persisitences.Outbox;
using SalesService.Domain.Entities;
using SalesService.Infrastructure.ApplicationDbContext;

namespace SalesService.Infrastructure.Repositories.Outbox;

/// <summary>
/// Triển khai interface IOutboxRepository.
/// </summary>
public class OutboxRepository : IOutboxRepository
{
    private readonly ISalesDbContext _context;

    public OutboxRepository(ISalesDbContext context)
    {
        _context = context;
    }

    public void Add(OutboxMessage message)
    {
        _context.OutboxMessages.Add(message);
    }
}
