using EngagementService.Application.Dtos.Campaign.Response;
using MediatR;

namespace EngagementService.Application.Features.Campaign.Queries.GetAll
{
    public record class GetAllQuery() : IRequest<IEnumerable<CampaignRes>>;
}