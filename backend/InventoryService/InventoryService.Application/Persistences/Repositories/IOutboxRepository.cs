using InventoryService.Domain.Entities;

namespace InventoryService.Application.Persistences.Repositories;

public interface IOutboxRepository
{
    void Add(OutboxMessage message);
}
