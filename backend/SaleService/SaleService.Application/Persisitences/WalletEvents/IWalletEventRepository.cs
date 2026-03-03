using SalesService.Domain.Entities;

namespace SalesService.Application.Persisitences.WalletEvents;

public interface IWalletEventRepository
{
    Task<Guid> AddAsync(WalletEvent walletEvent, CancellationToken cancellationToken);
    Task<WalletEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<WalletEvent>> GetPendingWithdrawsAsync(CancellationToken cancellationToken);
    Task<int> UpdateAsync(WalletEvent walletEvent, CancellationToken cancellationToken);
}
