using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetByIdsFilter
{
    public class GetByFilterQuery
        : IRequest<IEnumerable<Domain.Entities.LoyaltyAccount>>
    {
        public Guid? TierId { get; set; }
        public decimal? StartAt { get; set; }
        public decimal? EndAt { get; set; }
    }
}