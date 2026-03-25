using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using EngagementService.Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Infrastructure.Persistences.Repository
{
    public class VoucherUsageRepository : GenericRepository<VoucherUsage>, IVoucherUsageRepository
    {
        public VoucherUsageRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
        }

        public async Task AddRangeAsync(IEnumerable<VoucherUsage> entities, CancellationToken cancellationToken)
        {
           await _dbContext.VoucherUsages.AddRangeAsync(entities, cancellationToken);
        }

        public void DeleteRangeAsync(IEnumerable<VoucherUsage> entities, CancellationToken cancellationToken)
        {
             _dbContext.VoucherUsages.RemoveRange(entities);
        }

        public async Task<IEnumerable<VoucherUsage>> GetByOrderIdAsync(Guid id)
        {
            return await _dbContext.VoucherUsages
                                    .AsNoTracking()
                                    .Where(x => x.OrderId == id)
                                    .ToListAsync();
        }
    }
}
