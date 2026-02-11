using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Infrastructure.ApplicationDbContext;

namespace FranchiseService.Infrastructure.Repositories;

/// <summary>
/// Triển khai interface IOutboxRepository.
/// </summary>
public class OutboxRepository : IOutboxRepository
{
    private readonly IFranchiseDbContext _context;

    public OutboxRepository(IFranchiseDbContext context)
    {
        _context = context;
    }

    public void Add(OutboxMessage message)
    {
        _context.OutboxMessages.Add(message);
    }
}
