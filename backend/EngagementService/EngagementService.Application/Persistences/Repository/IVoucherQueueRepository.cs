using EngagementService.Application.Dtos.Voucher;
using EngagementService.Domain.Entities;

namespace EngagementService.Application.Persistences.Repository
{
    public interface IVoucherQueueRepository : IGenericRepository<VoucherQueue>
    {
        Task<int> CountVoucherInQueue(Guid voucherId, CancellationToken cancellationToken);
        Task AddQueueAsync(VoucherQueue voucherQueue, CancellationToken cancellationToken);

        Task<List<CountVoucherQueueRes>> GetVouchersAndAmountInQueueAsync(CancellationToken cancellationToken);
    }
}
