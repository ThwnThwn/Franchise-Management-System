using EngagementService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Persistences.Repository
{
    public interface IVoucherQueueRepository : IGenericRepository<VoucherQueue>
    {
        Task<int> CountVoucherInQueue(Guid voucherId, CancellationToken cancellationToken);
        Task AddQueueAsync(VoucherQueue voucherQueue, CancellationToken cancellationToken);
    }
}
