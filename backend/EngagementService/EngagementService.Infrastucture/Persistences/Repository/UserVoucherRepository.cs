using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
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
    public class UserVoucherRepository : GenericRepository<UserVoucher>, IUserVoucherRepository
    {
        public UserVoucherRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<UserVoucher>> GetByUserIdAsync(Guid id)
        {
            var userVouchers = await _dbSet
                .Include(uv => uv.Voucher)
                .ThenInclude(v => v.VoucherItems)
                .Where(uv => uv.UserId == id && uv.Status == UserVoucherStatus.AVAILABLE).ToListAsync();
            return userVouchers;
        }
    }
}
