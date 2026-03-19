using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.Campaign.Queries.GetActive;

public class GetActiveHandler(
    ICampaignRepository campaignRepository,
    IMapper mapper
) : IRequestHandler<GetActiveQuery, IEnumerable<CampaignRes>>
{
    public async Task<IEnumerable<CampaignRes>> Handle(GetActiveQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await campaignRepository.GetAllCampaign(
            predicate: c =>
                c.StartDate <= DateTimeOffset.UtcNow
                && c.EndDate >= DateTimeOffset.UtcNow
                && c.IsActive == true
        );

        return mapper.Map<IEnumerable<CampaignRes>>(campaigns);
    }
}