using EngagementService.Domain.Commons;
using EngagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EngagementService.Infrastructure.AppDbContext
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

        DbSet<PointRule> PointRules { get; set; }
        DbSet<Campaign> Campaigns { get; set; }

        DbSet<T> Set<T>() where T : class, IEntity;

        EntityEntry<T> Entry<T>(T entity) where T : class;

        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}