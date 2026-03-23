using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Persistences.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Campaign.Queries.GetById
{
    public class GetByIdHandler : IRequestHandler<GetByIdQuery, CampaignRes>
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IMapper _mapper;
        public GetByIdHandler(ICampaignRepository campaignRepository, IMapper mapper)
        {
            _campaignRepository = campaignRepository;
            _mapper = mapper;
        }

        public async Task<CampaignRes> Handle(GetByIdQuery request, CancellationToken cancellationToken)
        {
            var campaign = await _campaignRepository.GetCampaignById(request.id);

            if (campaign == null)
            {
                throw new ArgumentNullException($"campaign is null with id: {request.id} - GetById");
            }
            return _mapper.Map<CampaignRes>(campaign);
        }
    }
}

