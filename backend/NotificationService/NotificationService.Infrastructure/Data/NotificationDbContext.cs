using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
namespace NotificationService.Infrastructure.Data;
public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }
    public DbSet<NotificationLog> NotificationLogs { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Type)
                .IsRequired()
                .HasConversion<int>();
            
            entity.Property(e => e.Recipient)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.Subject)
                .HasMaxLength(500);
            
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();
            
            // Configure Metadata column to use PostgreSQL JSONB type
            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb");
            
            entity.HasIndex(e => e.Recipient);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}