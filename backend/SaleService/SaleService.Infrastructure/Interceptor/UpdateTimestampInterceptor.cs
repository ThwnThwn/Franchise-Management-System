using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SalesService.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SalesService.Infrastructure.Interceptor
{
    public class UpdateTimestampInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            UpdateModifiedEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateModifiedEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void UpdateModifiedEntities(DbContext? context)
        {
            if (context == null) return;
            var now = DateTimeOffset.UtcNow;
            var modifiedEntities = context.ChangeTracker
                .Entries()
                .Where(e =>
                    e.Entity is IEntity &&
                    e.State != EntityState.Deleted);


            foreach (var entry in modifiedEntities)
            {

                var entity = (IEntity)entry.Entity;
                if (entry.State == EntityState.Deleted)
                    continue;
                // Always update UpdatedAt for Modified + Added
                entity.UpdatedAt = now;

                // If newly added, also set CreatedAt
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = now;
                }
            }
        }
    }
}