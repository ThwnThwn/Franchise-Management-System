using InventoryService.Domain.Entities;
using SharedKernel.Abstractions;

namespace InventoryService.Application.Persistences.Repositories;

public interface IOutboxRepository : SharedKernel.Abstractions.IOutboxRepository<OutboxMessage>
{
    // Inherits Add(OutboxMessage message)
}

