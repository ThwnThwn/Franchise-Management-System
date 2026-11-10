using Microsoft.EntityFrameworkCore;
using SalesService.Application.Persisitences.WalletEvents;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.ApplicationDbContext;

namespace SalesService.Infrastructure.Repositories.WalletEvents;

public sealed class WalletEventRepository : IWalletEventRepository
{
    private readonly ISalesDbContext _db;

    public WalletEventRepository(ISalesDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> AddAsync(WalletEvent walletEvent, CancellationToken cancellationToken)
    {
        await _db.WalletEvents.AddAsync(walletEvent, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return walletEvent.Id;
    }

    public async Task<WalletEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.WalletEvents
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<WalletEvent>> GetPendingWithdrawsAsync(CancellationToken cancellationToken)
    {
        return await _db.WalletEvents
            .Where(e => e.EventType == WalletEventType.Withdraw
                     && e.Status == WalletEventStatus.Pending)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WalletEvent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _db.WalletEvents
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync(WalletEvent walletEvent, CancellationToken cancellationToken)
    {
        var existing = await GetByIdAsync(walletEvent.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"WalletEvent {walletEvent.Id} not found");

        _db.Entry(existing).CurrentValues.SetValues(walletEvent);
        _db.Entry(existing).Property(e => e.CreatedAt).IsModified = false;

        return await _db.SaveChangesAsync(cancellationToken);
    }
}