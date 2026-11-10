using MediatR;
using BuildingBlocks.Core.Pagination;
using EngagementService.Application.Persistences.Repository;

namespace EngagementService.Application.Features.PointLog.Queries
{
    public class ViewMyPointLogHandler : IRequestHandler<ViewMyPointLogQuery, PagedResult<Domain.Entities.PointLog>>
    {
        private readonly IPointLogRepository _pointLogRepository;
        public ViewMyPointLogHandler(IPointLogRepository pointLogRepository)
        {
            _pointLogRepository = pointLogRepository;
        }
        public async Task<PagedResult<Domain.Entities.PointLog>> Handle(ViewMyPointLogQuery request, CancellationToken cancellationToken)
        {
            var pagedIngredients = await _pointLogRepository.GetPagedPointLogsAsync(
                request.UserId,
                request.FromDate,
                request.ToDate,
                request.Page,
                request.PageSize,
                cancellationToken
            );

            return pagedIngredients;
        }
    }
}
