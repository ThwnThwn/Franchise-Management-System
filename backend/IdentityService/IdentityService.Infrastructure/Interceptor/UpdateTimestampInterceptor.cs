using IdentityService.Domain.Commons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Infrastructure.Interceptor
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
                .Where(e => e.State == EntityState.Modified && e.Entity is IEntity);

            foreach (var entry in modifiedEntities)
            {
                ((IEntity)entry.Entity).UpdatedAt = now;
            }
        }
    }
}
