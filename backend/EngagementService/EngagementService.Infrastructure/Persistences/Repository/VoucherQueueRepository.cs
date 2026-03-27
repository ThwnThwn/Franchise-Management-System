using EngagementService.Application.Dtos.Voucher;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using EngagementService.Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Infrastructure.Persistences.Repository
{
    public class VoucherQueueRepository : GenericRepository<VoucherQueue>, IVoucherQueueRepository
    {
        public VoucherQueueRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
        }
        /// <summary>
        /// Counts the voucher in queue.
        /// </summary>
        /// <param name="voucherId">The voucher identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<int> CountVoucherInQueue(Guid voucherId, CancellationToken cancellationToken)
        {
            return await _dbContext.VoucherQueues.CountAsync(vq => vq.VoucherId == voucherId);
        }
        /// <summary>
        /// Adds the queue asynchronous.
        /// </summary>
        /// <param name="voucherQueue">The voucher queue.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task AddQueueAsync(VoucherQueue voucherQueue, CancellationToken cancellationToken)
        {
            await _dbContext.VoucherQueues.AddAsync(voucherQueue, cancellationToken);
        }
        /// <summary>
        /// Gets the vouchers and amount in queue asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<CountVoucherQueueRes>> GetVouchersAndAmountInQueueAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.VoucherQueues
                .GroupBy(vq => vq.VoucherId)
                .Select(group => new CountVoucherQueueRes
                {
                    VoucherId = group.Key,
                    Count = group.Count()
                })
                .ToListAsync(cancellationToken);
        }
    }
}
