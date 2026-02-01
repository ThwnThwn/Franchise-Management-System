using InventoryService.Domain.Commons;
using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Infrastructure.ApplicationDbContext
{
     public interface IInventoryDbContext : IDisposable
    { 
        DbSet<Ingredient> Ingredients { get; set; }

        DbSet<Recipe> Recipes { get; set; }

        DbSet<StoreIngredient> StoreIngredients { get; set; }
        DbSet<OutboxMessage> OutboxMessages { get; set; }

        public DbSet<T> Set<T>() where T : class, IEntity;

        public EntityEntry<T> Entry<T>(T entity) where T : class;

        public int SaveChanges();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        }
}
