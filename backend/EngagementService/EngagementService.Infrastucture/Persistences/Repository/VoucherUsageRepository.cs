using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using EngagementService.Infrastucture.AppDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Infrastucture.Persistences.Repository
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
    }
}
