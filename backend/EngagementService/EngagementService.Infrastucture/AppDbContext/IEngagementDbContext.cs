using EngagementService.Domain.Commons;
using EngagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Infrastucture.AppDbContext
{
    public interface IEngagementDbContext : IDisposable
    {
         DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }

         DbSet<MembershipTier> MembershipTiers { get; set; }

         DbSet<PointLog> PointLogs { get; set; }

         DbSet<UserVoucher> UserVouchers { get; set; }

         DbSet<Voucher> Vouchers { get; set; }

         DbSet<VoucherItem> VoucherItems { get; set; }

         DbSet<VoucherQueue> VoucherQueues { get; set; }

         DbSet<VoucherUsage> VoucherUsages { get; set; }

        public DbSet<T> Set<T>() where T : class, IEntity;

        public EntityEntry<T> Entry<T>(T entity) where T : class;

        public int SaveChanges();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}
