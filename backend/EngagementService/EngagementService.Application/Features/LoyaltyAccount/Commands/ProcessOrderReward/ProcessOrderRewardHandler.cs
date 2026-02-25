using EngagementService.Application.Constants;
using EngagementService.Application.Features.LoyaltyAccount.Commands.Create;
using EngagementService.Application.Helper;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.LoyaltyAccount.Commands.ProcessOrderReward
{
    public class ProcessOrderRewardHandler : IRequestHandler<ProcessOrderRewardCommand, Unit>
    {
        private readonly IMembershipTierCache _membershipTierCache;
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IMembershipTierRepository _membershipTierRepository;
        public ProcessOrderRewardHandler(
            IMembershipTierCache membershipTierCache,
            ILoyaltyAccountRepository loyaltyAccountRepository,
            IMembershipTierRepository membershipTierRepository)
        {
            _membershipTierCache = membershipTierCache;
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _membershipTierRepository = membershipTierRepository;
        }
        public async Task<Unit> Handle(ProcessOrderRewardCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var loyaltyAccount = await _loyaltyAccountRepository.GetByIdAsync(request.Id);
            var point = await CalculatePointHelper.CalculatePoint(_membershipTierCache,loyaltyAccount!.CurrentTierId, request.OrderAmount);
            loyaltyAccount.TotalSpent += request.OrderAmount;
            loyaltyAccount.AvailablePoints += point;
            await _loyaltyAccountRepository.UpdateAsync(loyaltyAccount);
            return Unit.Value;

        }

    }
}
