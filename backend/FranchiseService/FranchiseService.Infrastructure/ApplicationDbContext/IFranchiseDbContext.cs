using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FranchiseService.Infrastructure.ApplicationDbContext
{
    public interface IFranchiseDbContext : IDisposable
    {
        DbSet<Franchisee> Franchisees { get; set; }

        DbSet<Schedule> Schedules { get; set; }

        DbSet<ShiftTemplate> ShiftTemplates { get; set; }

        DbSet<Store> Stores { get; set; }

        DbSet<StoreHour> StoreHours { get; set; }

        DbSet<TimeLog> TimeLogs { get; set; }

        public DbSet<T> Set<T>() where T : class, IEntity;

        public EntityEntry<T> Entry<T>(T entity) where T : class;

        public int SaveChanges();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}