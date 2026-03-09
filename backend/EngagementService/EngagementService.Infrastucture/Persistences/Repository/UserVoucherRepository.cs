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

        public async Task<List<UserVoucher>> GetByVoucherIdsAsync(List<Guid> voucherId, Guid userId)
        {
            var userVouchers = await _dbSet
                 .Include(uv => uv.Voucher)
                 .ThenInclude(v => v.VoucherItems)
                 .Where(uv => uv.Status == UserVoucherStatus.AVAILABLE && uv.UserId == userId).ToListAsync();

            var result = userVouchers.GroupBy(v => v.Voucher.Id)
                .Select(g =>
                {
                    return g.First();
                }).ToList();
            return result;
        }

        public void UpdateRangeAsync(List<UserVoucher> userVouchers, CancellationToken cancellationToken)
        {
            _dbSet.UpdateRange(userVouchers);
        }
    }
}
