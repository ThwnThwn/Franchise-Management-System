using BuildingBlocks.Core.Pagination;
using MediatR;

namespace EngagementService.Application.Features.PointLog.Queries
{
    public class ViewMyPointLogQuery : PaginationRequest, IRequest<PagedResult<Domain.Entities.PointLog>>
    {
        public Guid UserId { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}
