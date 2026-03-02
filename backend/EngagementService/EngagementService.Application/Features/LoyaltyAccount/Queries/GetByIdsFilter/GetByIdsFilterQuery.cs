using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetByIdsFilter
{
    public class GetByIdsFilterQuery
        : IRequest<IEnumerable<Domain.Entities.LoyaltyAccount>>
    {
        public List<Guid> UserIds { get; set; } = new();
        public Guid? TierId { get; set; }
        public decimal? StartAt { get; set; }
        public decimal? EndAt { get; set; }
    }
}