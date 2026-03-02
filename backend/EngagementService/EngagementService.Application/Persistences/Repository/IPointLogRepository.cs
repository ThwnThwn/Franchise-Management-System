using BuildingBlocks.Core.Pagination;
using EngagementService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Persistences.Repository
{
    public interface IPointLogRepository : IGenericRepository<PointLog>
    {
        Task<PagedResult<PointLog>> GetPagedPointLogsAsync(Guid userId, DateTimeOffset? fromDate, DateTimeOffset? toDate, int pageIndex, int pageSize, CancellationToken cancellationToken);
    }
}
