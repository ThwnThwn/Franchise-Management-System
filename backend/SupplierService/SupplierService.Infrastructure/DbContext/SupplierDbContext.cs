using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SupplierService.Domain.Commons;
using SupplierService.Domain.Entities;
using SupplierService.Infrastructure.Interceptor;
using System.Linq.Expressions;

namespace SupplierService.Infrastructure.DbContext;

public partial class SupplierDbContext : Microsoft.EntityFrameworkCore.DbContext, ISupplierDbContext
{
    private readonly UpdateTimestampInterceptor _updateInterceptor;

    public SupplierDbContext(DbContextOptions<SupplierDbContext> options, UpdateTimestampInterceptor updateTimestampInterceptor)
        : base(options)
    {
        _updateInterceptor = updateTimestampInterceptor;
    }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<SupplierIngredient> SupplierIngredients { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

    public virtual DbSet<SupplierPayment> SupplierPayments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_updateInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        // ── Supplier ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("suppliers_pkey");
            entity.ToTable("suppliers");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.ContactPerson).HasMaxLength(150).HasColumnName("contact_person");
            entity.Property(e => e.Phone).HasMaxLength(20).HasColumnName("phone");
            entity.Property(e => e.Email).HasMaxLength(150).HasColumnName("email");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.TaxCode).HasMaxLength(50).HasColumnName("tax_code");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
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
                .HasDatabaseName("ux_suppliers_code");
        });

        // ── SupplierIngredient ────────────────────────────────────────────────
        modelBuilder.Entity<SupplierIngredient>(entity =>
        {
            entity.HasKey(e => new { e.SupplierId, e.IngredientId });
            entity.ToTable("supplier_ingredients");

            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.IngredientId).HasColumnName("ingredient_id");
            entity.Property(e => e.IngredientName).HasMaxLength(255).HasColumnName("ingredient_name");
            entity.Property(e => e.IngredientUnit).HasMaxLength(50).HasColumnName("ingredient_unit");
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2).HasColumnName("unit_price");
            entity.Property(e => e.LeadTimeDays).HasColumnName("lead_time_days");
            entity.Property(e => e.IsPrimary).HasDefaultValue(false).HasColumnName("is_primary");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Supplier)
                .WithMany(s => s.SupplierIngredients)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── PurchaseOrder ─────────────────────────────────────────────────────
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("purchase_orders_pkey");
            entity.ToTable("purchase_orders");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status").HasConversion<int>();
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2).HasColumnName("total_amount");
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2).HasColumnName("discount_amount");
            entity.Property(e => e.NetAmount).HasPrecision(18, 2).HasColumnName("net_amount");
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2).HasColumnName("paid_amount");
            entity.Property(e => e.OrderDate).HasColumnName("order_date");
            entity.Property(e => e.ExpectedDeliveryDate).HasColumnName("expected_delivery_date");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.ReferenceCode).HasMaxLength(100).HasColumnName("reference_code");
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
                .HasDatabaseName("ux_purchase_orders_code");

            entity.HasOne(d => d.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_purchase_orders_supplier");
        });

        // ── PurchaseOrderItem ─────────────────────────────────────────────────
        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("purchase_order_items_pkey");
            entity.ToTable("purchase_order_items");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.IngredientId).HasColumnName("ingredient_id");
            entity.Property(e => e.Quantity).HasPrecision(12, 3).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2).HasColumnName("unit_price");
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2).HasColumnName("total_price");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at")
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.PurchaseOrder)
                .WithMany(po => po.Items)
                .HasForeignKey(d => d.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_purchase_order_items_po");
        });

        // ── SupplierPayment ───────────────────────────────────────────────────
        modelBuilder.Entity<SupplierPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("supplier_payments_pkey");
            entity.ToTable("supplier_payments");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
            entity.Property(e => e.Amount).HasPrecision(18, 2).HasColumnName("amount");
            entity.Property(e => e.Method).HasColumnName("method");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Provider).HasMaxLength(50).HasColumnName("provider");
            entity.Property(e => e.ProviderTxnId).HasMaxLength(100).HasColumnName("provider_txn_id");
            entity.Property(e => e.ProviderPayload).HasColumnName("provider_payload");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
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
                .HasDatabaseName("ux_supplier_payments_code");

            entity.HasOne(d => d.PurchaseOrder)
                .WithMany(po => po.Payments)
                .HasForeignKey(d => d.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_supplier_payments_po");
        });

        // ── Global Query Filter: soft-delete (giống SalesService) ─────────────
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
