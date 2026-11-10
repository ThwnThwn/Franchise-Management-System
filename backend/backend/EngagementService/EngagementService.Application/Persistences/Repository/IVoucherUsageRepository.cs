using EngagementService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Persistences.Repository
{
    public interface IVoucherUsageRepository : IGenericRepository<VoucherUsage>
    {
        Task AddRangeAsync(IEnumerable<VoucherUsage> entities, CancellationToken cancellationToken);

        Task<IEnumerable<VoucherUsage>> GetByOrderIdAsync(Guid id);
        

        void DeleteRangeAsync(IEnumerable<VoucherUsage> entities, CancellationToken cancellationToken);
    }
}
