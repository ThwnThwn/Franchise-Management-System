using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Persistences.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Campaign.Queries.GetAll
{
    public class GetAllHandler : IRequestHandler<GetAllQuery, IEnumerable<CampaginRes>>
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IMapper _mapper;

        public GetAllHandler(ICampaignRepository campaignRepository, IMapper mapper)
        {
            _campaignRepository = campaignRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CampaginRes>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            var campaigns = await _campaignRepository.GetAllCampaign();

            return _mapper.Map<IEnumerable<CampaginRes>>(campaigns);
        }
    }
}
