using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using MediatR;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using System;
using System.Runtime.ConstrainedExecution;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace EngagementService.Application.EventHandler
{
    public class RedeemCampaignVoucherEventHanlder : IIntegrationEventHandler<RedeemCampaignVoucherEvent>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IPointRuleCache _pointRuleCache;
        private readonly IMediator _mediator;
        public RedeemCampaignVoucherEventHanlder(
            IVoucherRepository voucherRepository,
            ILoyaltyAccountRepository loyaltyAccountRepository,
            IPointRuleCache pointRuleCache,
            IMediator mediator)
        {
            _voucherRepository = voucherRepository;
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _pointRuleCache = pointRuleCache;
            _mediator = mediator;
        }

        public async Task Handle(RedeemCampaignVoucherEvent @event)
        {

            var voucher = await _voucherRepository.GetByIdAsync(@event.VoucherId);
            var loyalty = await _loyaltyAccountRepository.GetByUserIdAsync(@event.UserId);

            if (voucher == null || loyalty == null)
            {
                return;
            }

            //Idempotency Check: Ensure don't add the same voucher twice 
            var alreadyExists = loyalty.UserVouchers.Any(uv => uv.VoucherId == @event.VoucherId);
            if (alreadyExists) return;

            //Update Database State
            voucher.CurrentQuantity -= 1;
            loyalty.UserVouchers.Add(new UserVoucher
            {
                VoucherId = voucher.Id,
                UserId = loyalty.Id,
                AcquiredAt = DateTimeOffset.UtcNow,
                Voucher = voucher,
            });

            //Persist changes
            await _voucherRepository.UpdateAsync(voucher);
            await _loyaltyAccountRepository.UpdateAsync(loyalty);
        }
    }
}