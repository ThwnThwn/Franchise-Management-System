using AutoMapper;
using EngagementService.Application.AppExceptions;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Persistences.Repository;
using IdentityService.Application.Abstrations;
using MediatR;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace EngagementService.Application.Features.Campaign.Commands.RedeemCampaignVoucher;

public class RedeemCampaignVoucherHandler(
    ICampaignRepository campaignRepository,
    IMapper mapper,
    ICampaignCache campaignCache,
    ILoyaltyAccountRepository loyaltyAccountRepository,
    IVoucherRepository voucherRepository,
    IKafkaProducer kafkaProducer
) : IRequestHandler<RedeemCampaignVoucherCommand, CampaignRes>
{
    public async Task<CampaignRes> Handle(RedeemCampaignVoucherCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var voucher = await voucherRepository.GetByIdAsync(request.VoucherId)
            ?? throw new BusinessException($"voucher does not exit {request.VoucherId}.");

        if (voucher.CampaignId == null)
        {
            throw new BusinessException($"voucher does not has this campaign {voucher.CampaignId}.");
        }

        var loyaltyAccount = await loyaltyAccountRepository.GetByUserIdAsync(request.UserId)
            ?? throw new BusinessException($"loyalty account does not exit for user {request.UserId}.");

        if (voucher.PointCost != null
            && voucher.PointCost > 0
            && loyaltyAccount.AvailablePoints < (voucher.PointCost ?? 0)
        )
        {
            throw new BusinessException($"not enough point to redeem this voucher, required: {voucher.PointCost}, available: {loyaltyAccount.AvailablePoints}.");
        }

        //Atomic Check & Decrement in Redis
        var result = await campaignCache.RedeemVoucherAsync(request.VoucherId, request.UserId);

        //Handle Redis Errors
        if (result == -1) throw new BusinessException("User already has this voucher.");
        if (result == 0) throw new BusinessException("Voucher out of stock.");

        //If result == 1 (Success in Cache)
        if (result == 1)
        {
            // PUSH TO KAFKA: Let the background worker update SQL
            await kafkaProducer.PublishAsync(KafkaTopics.RedeemCampaignVoucher, new RedeemCampaignVoucherEvent
            {
                UserId = request.UserId,
                VoucherId = request.VoucherId
            });

            if (voucher.PointCost != null && voucher.PointCost > 0)
            {
                await kafkaProducer.PublishAsync(KafkaTopics.UpdateUserPoint, new UpdatePointEvent
                {
                    Id = request.UserId,
                    ActionType = UpdatePointType.Minus,
                    OrderId = request.VoucherId,
                    Point = voucher.PointCost ?? 0
                });
            }
        }

        var campaign = await campaignRepository.GetCampaignById(voucher.CampaignId.Value);
        return mapper.Map<CampaignRes>(campaign);
    }
}