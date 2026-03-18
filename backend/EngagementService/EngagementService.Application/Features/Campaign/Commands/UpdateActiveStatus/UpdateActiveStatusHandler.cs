using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Persistences.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Campaign.Commands.UpdateActiveStatus
{
    public class UpdateActiveStatusHandler : IRequestHandler<UpdateActiveStatusCommand, CampaginRes>
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IMapper _mapper;

        public UpdateActiveStatusHandler(ICampaignRepository campaignRepository, IMapper mapper)
        {
            _campaignRepository = campaignRepository;
            _mapper = mapper;
        }

        public async Task<CampaginRes> Handle(UpdateActiveStatusCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            if(request.Id == null)
            {
                throw new ArgumentNullException("Id is null" +nameof(command));
            }
            Domain.Entities.Campaign campaign = await _campaignRepository.GetCampaignById(request.Id);
            if(campaign == null)
            {
                throw new ArgumentNullException($"campign with Id {request.Id} is null" + nameof(command));
            }
            campaign.IsActive = request.IsActive;
            campaign.Vouchers.ToList().ForEach(v => v.IsActive = request.IsActive);

            await _campaignRepository.UpdateAsync(campaign);
            return _mapper.Map<CampaginRes>(campaign);
        }
    }
}
