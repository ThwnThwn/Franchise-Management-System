using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using EngagementService.Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Infrastructure.Persistences.Repository
{
    public class PointLogRepository : GenericRepository<PointLog>, IPointLogRepository
    {
        public PointLogRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
        }
        /// <summary>
        /// Gets the paged point logs asynchronous.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<PagedResult<PointLog>> GetPagedPointLogsAsync(Guid userId, DateTimeOffset? fromDate, DateTimeOffset? toDate, int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            var query = _dbContext.PointLogs
                .Include(i => i.User)
                .AsNoTracking()
                .Where(p => p.User!.Id == userId && p.ExpiredAt > DateTimeOffset.UtcNow);

            if (fromDate != null)
            {
                query = query.Where(i => i.CreatedAt >= fromDate );
            }

            if (toDate != null)
            {
                query = query.Where(i => i.CreatedAt <= toDate);
            }
            query = query.OrderByDescending(u => u.CreatedAt);

            return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
        }
    }
}
