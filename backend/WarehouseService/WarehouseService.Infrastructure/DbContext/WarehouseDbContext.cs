using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using WarehouseService.Domain.Commons;
using WarehouseService.Domain.Entities;
using WarehouseService.Infrastructure.Interceptor;

namespace WarehouseService.Infrastructure.DbContext;

public partial class WarehouseDbContext : Microsoft.EntityFrameworkCore.DbContext, IWarehouseDbContext
{
    private readonly UpdateTimestampInterceptor _updateInterceptor;

    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options, UpdateTimestampInterceptor updateTimestampInterceptor)
        : base(options)
    {
        _updateInterceptor = updateTimestampInterceptor;
    }

    public virtual DbSet<Warehouse> Warehouses { get; set; }
    public virtual DbSet<WarehouseItem> WarehouseItems { get; set; }
    public virtual DbSet<StockTransaction> StockTransactions { get; set; }
    public virtual DbSet<WarehouseTransfer> WarehouseTransfers { get; set; }
    public DbSet<WarehouseTransferItem> WarehouseTransferItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_updateInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        // ── Warehouse ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("warehouses_pkey");
            entity.ToTable("warehouses");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at")
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasIndex(e => e.Code)
                .IsUnique()
                .HasFilter("is_deleted = FALSE")
                .HasDatabaseName("ux_warehouses_code");
        });

        // ── WarehouseItem ─────────────────────────────────────────────────────
        modelBuilder.Entity<WarehouseItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("warehouse_items_pkey");
            entity.ToTable("warehouse_items");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.IngredientId).HasColumnName("ingredient_id");
            entity.Property(e => e.Unit).HasMaxLength(50).HasColumnName("unit");
            entity.Property(e => e.Quantity).HasPrecision(12, 3).HasColumnName("quantity");
            entity.Property(e => e.BasePrice).HasPrecision(18, 2).HasColumnName("base_price");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at")
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasIndex(e => new { e.WarehouseId, e.IngredientId })
                .IsUnique()
                .HasFilter("is_deleted = FALSE")
                .HasDatabaseName("ux_warehouse_items_warehouse_ingredient");

            entity.HasOne(d => d.Warehouse)
                .WithMany(w => w.WarehouseItems)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_warehouse_items_warehouse");
        });

        // ── StockTransaction ──────────────────────────────────────────────────
        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stock_transactions_pkey");
            entity.ToTable("stock_transactions");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.TransactionCode).HasMaxLength(50).HasColumnName("transaction_code");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.WarehouseItemId).HasColumnName("warehouse_item_id");
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<int>();
            entity.Property(e => e.Quantity).HasPrecision(12, 3).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2).HasColumnName("unit_price");
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2).HasColumnName("total_price");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.ReferenceCode).HasMaxLength(100).HasColumnName("reference_code");
            entity.Property(e => e.WarehouseTransferId).HasColumnName("warehouse_transfer_id");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at")
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasIndex(e => e.TransactionCode)
                .IsUnique()
                .HasFilter("is_deleted = FALSE")
                .HasDatabaseName("ux_stock_transactions_code");

            entity.HasOne(d => d.Warehouse)
                .WithMany(w => w.StockTransactions)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_stock_transactions_warehouse");

            entity.HasOne(d => d.WarehouseItem)
                .WithMany(wi => wi.StockTransactions)
                .HasForeignKey(d => d.WarehouseItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_stock_transactions_warehouse_item");

            entity.HasOne(d => d.WarehouseTransfer)
                .WithMany(wt => wt.StockTransactions)
                .HasForeignKey(d => d.WarehouseTransferId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_stock_transactions_transfer");
        });

        // ── WarehouseTransfer ─────────────────────────────────────────────────
        modelBuilder.Entity<WarehouseTransfer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("warehouse_transfers_pkey");
            entity.ToTable("warehouse_transfers");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.TransferCode).HasMaxLength(50).HasColumnName("transfer_code");
            entity.Property(e => e.FromWarehouseId).HasColumnName("from_warehouse_id");
            entity.Property(e => e.ToWarehouseId).HasColumnName("to_warehouse_id");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at")
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasIndex(e => e.TransferCode)
                .IsUnique()
                .HasFilter("is_deleted = FALSE")
                .HasDatabaseName("ux_warehouse_transfers_code");

            entity.HasOne(d => d.FromWarehouse)
                .WithMany(w => w.TransfersOut)
                .HasForeignKey(d => d.FromWarehouseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_warehouse_transfers_from");

            entity.HasOne(d => d.ToWarehouse)
                .WithMany(w => w.TransfersIn)
                .HasForeignKey(d => d.ToWarehouseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_warehouse_transfers_to");
        });

        // ── WarehouseTransferItem ─────────────────────────────────────────────────
        modelBuilder.Entity<WarehouseTransferItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("warehouse_transfer_items_pkey");
            entity.ToTable("warehouse_transfer_items");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.WarehouseTransferId).HasColumnName("warehouse_transfer_id");
            entity.Property(e => e.WarehouseItemId).HasColumnName("warehouse_item_id");
            entity.Property(e => e.IngredientId).HasColumnName("ingredient_id");
            entity.Property(e => e.Quantity).HasPrecision(12, 3).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2).HasColumnName("unit_price");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at")
                .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");

            entity.HasOne(d => d.WarehouseTransfer)
                .WithMany(wt => wt.TransferItems)
                .HasForeignKey(d => d.WarehouseTransferId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_transfer_items_transfer");

            entity.HasOne(d => d.WarehouseItem)
                .WithMany()
                .HasForeignKey(d => d.WarehouseItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_transfer_items_warehouse_item");
        });

        // ── Global Query Filter: soft-delete ──────────────────────────────────
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(SoftDeletedEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GenerateIsDeletedExpression(entityType.ClrType));
            }
        }

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    private static LambdaExpression GenerateIsDeletedExpression(Type type)
    {
        var parameter = Expression.Parameter(type, "e");
        var property = Expression.Property(parameter, nameof(SoftDeletedEntity.IsDeleted));
        var constant = Expression.Constant(false);
        var compare = Expression.Equal(property, constant);
        return Expression.Lambda(compare, parameter);
    }
}