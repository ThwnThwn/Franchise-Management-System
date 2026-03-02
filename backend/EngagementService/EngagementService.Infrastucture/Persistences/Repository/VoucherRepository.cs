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
    public class VoucherRepository : GenericRepository<Voucher>, IVoucherRepository
    {
        public VoucherRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Voucher?> GetVoucherByCodeAsync(string code)
        {
           return await _dbContext.Vouchers
                .Where(v => v.Code == code)
                .FirstOrDefaultAsync();
        }
    }
}
