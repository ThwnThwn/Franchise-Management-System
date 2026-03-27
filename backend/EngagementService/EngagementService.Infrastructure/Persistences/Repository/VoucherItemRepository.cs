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
    public class VoucherItemRepository : GenericRepository<VoucherItem>, IVoucherItemRepository
    {
        public VoucherItemRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<VoucherItem>> GetByVoucherId(Guid voucherId)
        {
            return await _dbContext.VoucherItems
                .Where(v => v.VoucherId == voucherId && v.IsDeleted == false)
                .ToListAsync();
        }
    }
}
