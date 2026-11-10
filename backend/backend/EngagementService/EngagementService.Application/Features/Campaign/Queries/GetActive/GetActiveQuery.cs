using EngagementService.Application.Dtos.Campaign.Response;
using MediatR;

namespace EngagementService.Application.Features.Campaign.Queries.GetActive
{
    public record class GetActiveQuery() : IRequest<IEnumerable<CampaignRes>>;
}