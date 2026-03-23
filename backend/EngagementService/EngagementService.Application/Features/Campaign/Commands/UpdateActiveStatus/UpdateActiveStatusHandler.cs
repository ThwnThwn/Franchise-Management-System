using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Persistences.Repository;
using IdentityModel.Client;
using IdentityService.Application.Abstrations;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Campaign.Commands.UpdateActiveStatus
{
    public class UpdateActiveStatusHandler : IRequestHandler<UpdateActiveStatusCommand, CampaignRes>
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IMapper _mapper;
        private readonly ICampaignCache _campaignCache;
        

        public UpdateActiveStatusHandler(ICampaignRepository campaignRepository, IMapper mapper, ICampaignCache campaignCache)
        {
            _campaignRepository = campaignRepository;
            _mapper = mapper;
            _campaignCache = campaignCache;
        }

        public async Task<CampaignRes> Handle(UpdateActiveStatusCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            if(request.Id == Guid.Empty)
            {
                throw new ArgumentNullException("Id is null" +nameof(command));
            }
            Domain.Entities.Campaign? campaign = await _campaignRepository.GetCampaignById(request.Id);
            if(campaign == null)
            {
                throw new ArgumentNullException($"campign with Id {request.Id} is null" + nameof(command));
            }
            campaign.IsActive = request.IsActive;
            campaign.Vouchers.ToList().ForEach(v => v.IsActive = request.IsActive);
            await _campaignRepository.UpdateAsync(campaign);

            // If true then push to redis || If false then delete
            if (request.IsActive == true)
            {
                // Push to Redis CAMP_DB 3
                foreach (var voucher in campaign.Vouchers)
                {
                    await _campaignCache.WarmUpVoucherAsync(
                        voucher.Id,
                        voucher.InitQuantity ?? 0,
                        campaign.EndDate);
                }
            }
            else
            {
                // CLEANUP: Remove from Redis so no one can redeem while inactive
                foreach (var voucher in campaign.Vouchers)
                {
                    await _campaignCache.DeleteVoucherCacheAsync(voucher.Id);
                }
            }

            return _mapper.Map<CampaignRes>(campaign);
        }
    }
}
