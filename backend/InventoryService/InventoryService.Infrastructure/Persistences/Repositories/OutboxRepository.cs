using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using InventoryService.Application.Abstractions;

namespace InventoryService.Infrastructure.Persistences.Repositories;

/// <summary>
/// Triển khai IOutboxRepository cho Inventory Service.
/// </summary>
public class OutboxRepository : IOutboxRepository
{
    private readonly IInventoryDbContext _context;

    public OutboxRepository(IInventoryDbContext context)
    {
        _context = context;
    }

    public void Add(OutboxMessage message)
    {
        _context.OutboxMessages.Add(message);
    }
}
