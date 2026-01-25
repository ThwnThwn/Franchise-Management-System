using System;
using Microsoft.EntityFrameworkCore;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Infrastructure.Data;

public partial class SalesDbContext : DbContext, ISalesDbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");
            entity.ToTable("orders");
            entity.HasIndex(e => e.Code, "idx_orders_code").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");
            entity.Property(e => e.Notes).HasMaxLength(255).HasColumnName("notes");
            entity.Property(e => e.PaidAmount).HasPrecision(12, 2).HasColumnName("paid_amount");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethod).HasConversion<int>().HasColumnName("payment_method");
            entity.Property(e => e.Status).HasConversion<int>().HasDefaultValue(OrderStatus.Pending).HasColumnName("status");
            entity.Property(e => e.Subtotal).HasPrecision(12, 2).HasColumnName("subtotal");
            entity.Property(e => e.Tax).HasPrecision(12, 2).HasDefaultValue(0m).HasColumnName("tax");
            entity.Property(e => e.TotalAmount).HasPrecision(12, 2).HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_items_pkey");
            entity.ToTable("order_items");
            entity.HasIndex(e => e.OrderId, "idx_order_items_order");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.ImageUrl).HasMaxLength(500).HasColumnName("image_url");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName).HasMaxLength(255).HasColumnName("product_name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Sku).HasMaxLength(100).HasColumnName("sku");
            entity.Property(e => e.UnitPrice).HasPrecision(12, 2).HasColumnName("unit_price");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_order_items_order");
        });
    }
}
