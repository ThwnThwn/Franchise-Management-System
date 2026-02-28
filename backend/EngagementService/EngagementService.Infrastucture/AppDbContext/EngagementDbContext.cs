using EngagementService.Domain.Commons;
using EngagementService.Domain.Constants;
using EngagementService.Domain.Entities;
using EngagementService.Infrastucture.Interceptor;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EngagementService.Infrastucture.AppDbContext;

public partial class EngagementDbContext : DbContext, IEngagementDbContext
{
    private readonly UpdateTimestampInterceptor _updateInterceptor;
    public EngagementDbContext()
    {
    }

    public EngagementDbContext(DbContextOptions<EngagementDbContext> options, UpdateTimestampInterceptor updateInterceptor)
        : base(options)
    {
        _updateInterceptor = updateInterceptor; 
    }

    public virtual DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }

    public virtual DbSet<MembershipTier> MembershipTiers { get; set; }

    public virtual DbSet<PointLog> PointLogs { get; set; }

    public virtual DbSet<UserVoucher> UserVouchers { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<VoucherItem> VoucherItems { get; set; }

    public virtual DbSet<VoucherQueue> VoucherQueues { get; set; }

    public virtual DbSet<VoucherUsage> VoucherUsages { get; set; }

    public DbSet<T> Set<T>() where T : class, IEntity => base.Set<T>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_updateInterceptor);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Đăng ký các Postgres Enum bằng kiểu dữ liệu C# (Generic)
        // Việc này giúp EF Core nhận diện các kiểu Enum tùy chỉnh trong PostgreSQL
        modelBuilder.HasPostgresEnum<PointActionType>("point_action_type");
        modelBuilder.HasPostgresEnum<PointReasonCode>("point_reason_code");
        modelBuilder.HasPostgresEnum<UserVoucherStatus>("user_voucher_status");
        modelBuilder.HasPostgresEnum<VoucherCategory>("voucher_category");
        modelBuilder.HasPostgresEnum<VoucherDiscountType>("voucher_discount_type");
        modelBuilder.HasPostgresEnum<VoucherItemType>("voucher_item_type");

        modelBuilder.HasPostgresExtension("uuid-ossp");

        base.OnModelCreating(modelBuilder);

        // --- NHÓM 1: QUẢN LÝ ĐIỂM VÀ HẠNG ---

        modelBuilder.Entity<LoyaltyAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("loyalty_accounts_pkey");
            entity.ToTable("loyalty_accounts");
            entity.HasIndex(e => e.CurrentTierId, "idx_loyalty_accounts_tier");

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.AvailablePoints).HasPrecision(15, 3).HasDefaultValueSql("0").HasColumnName("available_points");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.CurrentTierId).HasColumnName("current_tier_id");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");
            entity.Property(e => e.TotalSpent).HasPrecision(15, 3).HasDefaultValueSql("0").HasColumnName("total_spent");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");

            entity.HasOne(d => d.CurrentTier).WithMany(p => p.LoyaltyAccounts)
                .HasForeignKey(d => d.CurrentTierId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_loyalty_tier");

            // Global Filter cho Soft Delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<MembershipTier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("membership_tiers_pkey");
            entity.ToTable("membership_tiers");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.DiscountRate).HasDefaultValueSql("0.0").HasColumnName("discount_rate");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");
            entity.Property(e => e.MinSpendRequired).HasPrecision(15, 3).HasColumnName("min_spend_required");
            entity.Property(e => e.ValuePerPoint).HasPrecision(15, 3).HasColumnName("value_per_point");
            entity.Property(e => e.AmountPerPoint).HasPrecision(15, 3).HasColumnName("amount_per_point");
            entity.Property(e => e.Name).HasMaxLength(20).HasColumnName("name");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<PointLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("point_logs_pkey");
            entity.ToTable("point_logs");
            entity.HasIndex(e => e.UserId, "idx_point_logs_user_id");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");

            // Cấu hình Mapping Enum cho PointLog
            entity.Property(e => e.ActionType)
                .HasColumnType("point_action_type") // Ép kiểu cột về đúng Enum trong DB
                .HasColumnName("action_type");

            entity.Property(e => e.ReasonCode)
                .HasColumnType("point_reason_code")
                .HasColumnName("reason_code");

            entity.Property(e => e.Amount).HasPrecision(15, 3).HasColumnName("amount");
            entity.Property(e => e.BalanceAfter).HasPrecision(15, 3).HasColumnName("balance_after");
            entity.Property(e => e.BalanceBefore).HasPrecision(15, 3).HasColumnName("balance_before");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.PointLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("point_logs_user_id_fkey");
        });

        // --- NHÓM 2: QUẢN LÝ VOUCHER ---

        modelBuilder.Entity<UserVoucher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_vouchers_pkey");
            entity.ToTable("user_vouchers");
            entity.HasIndex(e => new { e.UserId, e.Status }, "idx_user_vouchers_user_status");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.AcquiredAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("acquired_at");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");

            // Cấu hình Mapping Enum cho Status
            entity.Property(e => e.Status)
                .HasColumnType("user_voucher_status")
                .HasDefaultValueSql("'AVAILABLE'::user_voucher_status")
                .HasColumnName("status");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserVouchers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_vouchers_user_id_fkey");

            entity.HasOne(d => d.Voucher).WithMany(p => p.UserVouchers)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_vouchers_voucher_id_fkey");

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("vouchers_pkey");
            entity.ToTable("vouchers");
            entity.HasIndex(e => e.Code, "uq_voucher_code_active").IsUnique().HasFilter("(is_deleted IS FALSE)");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");

            // Cấu hình Mapping Enum cho Voucher
            entity.Property(e => e.Category)
                .HasColumnType("voucher_category")
                .HasColumnName("category");

            entity.Property(e => e.Type)
                .HasColumnType("voucher_discount_type")
                .HasColumnName("type");

            entity.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.CurrentQuantity).HasDefaultValue(0).HasColumnName("current_quantity");
            entity.Property(e => e.DiscountValue).HasPrecision(15, 3).HasColumnName("discount_value");
            entity.Property(e => e.MaxDiscountValue).HasPrecision(15, 3).HasColumnName("max_discount_value");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.InitQuantity).HasDefaultValue(0).HasColumnName("init_quantity");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");
            entity.Property(e => e.MaxUsagePerUser).HasDefaultValue(1).HasColumnName("max_usage_per_user");
            entity.Property(e => e.MinOrderValue).HasPrecision(15, 3).HasDefaultValueSql("0").HasColumnName("min_order_value");
            entity.Property(e => e.PointCost).HasPrecision(15, 3).HasDefaultValueSql("0").HasColumnName("point_cost");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");

            // Ràng buộc kiểm tra số lượng (CHECK Constraint)
            entity.HasCheckConstraint("check_quantity_logic", "\"current_quantity\" <= \"init_quantity\"");

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<VoucherItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("voucher_items_pkey");
            entity.ToTable("voucher_items");
            entity.HasIndex(e => e.VoucherId, "idx_voucher_items_lookup").HasFilter("(is_deleted IS FALSE)");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");
            entity.Property(e => e.Quantity).HasDefaultValue(1).HasColumnName("quantity");

            // Cấu hình Mapping Enum cho VoucherItemType
            entity.Property(e => e.Type)
                .HasColumnType("voucher_item_type")
                .HasDefaultValueSql("'REQUIRED'::voucher_item_type")
                .HasColumnName("type");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");

            entity.HasOne(d => d.Voucher).WithMany(p => p.VoucherItems)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("voucher_items_voucher_id_fkey");

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<VoucherQueue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("voucher_queue_pkey");
            entity.ToTable("voucher_queue");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");

            entity.HasOne(d => d.Voucher).WithMany(p => p.VoucherQueues)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("voucher_queue_voucher_id_fkey");
        });

        modelBuilder.Entity<VoucherUsage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("voucher_usages_pkey");
            entity.ToTable("voucher_usages");
            entity.HasIndex(e => e.OrderId, "idx_voucher_usages_order_id");
            entity.HasIndex(e => e.StoreId, "idx_voucher_usages_store_id");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.AppliedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("applied_at");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.DiscountAmount).HasPrecision(15, 3).HasDefaultValueSql("0").HasColumnName("discount_amount");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.UserVoucherId).HasColumnName("user_voucher_id");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");

            entity.HasOne(d => d.UserVoucher).WithMany(p => p.VoucherUsages)
                .HasForeignKey(d => d.UserVoucherId)
                .HasConstraintName("voucher_usages_user_voucher_id_fkey");

            entity.HasOne(d => d.Voucher).WithMany(p => p.VoucherUsages)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("voucher_usages_voucher_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
