using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using EngagementService.Infrastucture.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Infrastucture.Persistences.Repository
{
    public class VoucherQueueRepository : GenericRepository<VoucherQueue>, IVoucherQueueRepository
    {
        public VoucherQueueRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<int> CountVoucherInQueue(Guid voucherId, CancellationToken cancellationToken)
        {
            return await _dbContext.VoucherQueues.CountAsync(vq => vq.VoucherId == voucherId);
        }

        public async Task AddQueueAsync(VoucherQueue voucherQueue, CancellationToken cancellationToken)
        {
            await _dbContext.VoucherQueues.AddAsync(voucherQueue, cancellationToken);
        }
    }
}
