using AutoMapper;
using EngagementService.Application.AppExceptions;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using IdentityService.Application.Abstrations;
using MediatR;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Campaign.Commands.RedeemCampaignVoucher
{
    public class RedeemCampaignVoucherHandler : IRequestHandler<RedeemCampaignVoucherCommand, CampaignRes>
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IMapper _mapper;
        private readonly ICampaignCache _campaignCache;
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IKafkaProducer _kafkaProducer;

        public RedeemCampaignVoucherHandler(
            ICampaignRepository campaignRepository, 
            IMapper mapper, 
            ICampaignCache campaignCache, 
            ILoyaltyAccountRepository loyaltyAccountRepository, 
            IVoucherRepository voucherRepository,
            IKafkaProducer kafkaProducer)
        {
            _campaignRepository = campaignRepository;
            _mapper = mapper;
            _campaignCache = campaignCache;
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _voucherRepository = voucherRepository;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<CampaignRes> Handle(RedeemCampaignVoucherCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            //Atomic Check & Decrement in Redis
            var result = await _campaignCache.RedeemVoucherAsync(request.VoucherId, request.UserId);

            //Handle Redis Errors
            if (result == -1) throw new BusinessException("User already has this voucher.");
            if (result == 0) throw new BusinessException("Voucher out of stock.");

            //If result == 1 (Success in Cache)
            if (result == 1)
            {
                // PUSH TO KAFKA: Let the background worker update SQL
                await _kafkaProducer.PublishAsync(KafkaTopics.RedeemCampaignVoucher, new RedeemCampaignVoucherEvent
                {
                    UserId = request.UserId,
                    VoucherId = request.VoucherId
                });
            }

            var voucher = await _voucherRepository.GetByIdAsync(request.VoucherId);
            var campaign = await _campaignRepository.GetCampaignById(voucher.CampaignId.Value);
            return _mapper.Map<CampaignRes>(campaign);
        }
    }
}
