using System;
using System.Collections.Generic;
using InventoryService.Application.Abstractions;
using InventoryService.Domain.Commons;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Interceptor;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.ApplicationDbContext;

public partial class InventoryDbContext : DbContext, IInventoryDbContext
{
    private readonly UpdateTimestampInterceptor _updateTimestampInterceptor;
    public InventoryDbContext()
    {
    }

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options, UpdateTimestampInterceptor updateTimestampInterceptor)
        : base(options)
    {
        _updateTimestampInterceptor = updateTimestampInterceptor;
    }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<StoreIngredient> StoreIngredients { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    public DbSet<T> Set<T>() where T : class, IEntity => base.Set<T>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_updateTimestampInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ingredients_pkey");

            entity.ToTable("ingredients");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.BaseUnit)
                .HasMaxLength(50)
                .HasColumnName("base_unit");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("recipes_pkey");

            entity.ToTable("recipes");

            entity.HasIndex(e => new { e.VariantId, e.IngredientId }, "unique_variant_ingredient").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IngredientId).HasColumnName("ingredient_id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Quantity)
                .HasPrecision(12, 3)
                .HasColumnName("quantity");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.IngredientId)
                .HasConstraintName("fk_recipes_ingredient");
        });

        modelBuilder.Entity<StoreIngredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("store_ingredients_pkey");

            entity.ToTable("store_ingredients");

            entity.HasIndex(e => new { e.StoreId, e.IngredientId }, "unique_store_ingredient").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IngredientId).HasColumnName("ingredient_id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.MinThreshold)
                .HasPrecision(12, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("min_threshold");
            entity.Property(e => e.Quantity)
                .HasPrecision(12, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("quantity");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.StoreIngredients)
                .HasForeignKey(d => d.IngredientId)
                .HasConstraintName("fk_store_ingredients_ingredient");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
