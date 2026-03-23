using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.Campaign.Queries.GetAll
{
    public class GetActiveHandler : IRequestHandler<GetAllQuery, IEnumerable<CampaignRes>>
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IMapper _mapper;

        public GetActiveHandler(ICampaignRepository campaignRepository, IMapper mapper)
        {
            _campaignRepository = campaignRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CampaignRes>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            var campaigns = await _campaignRepository.GetAllCampaign();

            return _mapper.Map<IEnumerable<CampaignRes>>(campaigns);
        }
    }
}