using EngagementService.Application.Constants;
using EngagementService.Application.Helper;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using MediatR;

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
            var point = await CalculatePointHelper.CalculatePoint(_membershipTierRepository, _membershipTierCache, loyaltyAccount!.CurrentTierId, request.OrderAmount);

            //Update
            loyaltyAccount.TotalSpent += request.OrderAmount;
            loyaltyAccount.AvailablePoints += point;

            //Lấy tier mới nếu có
            var newTierId = await CheckTierUpgrade(loyaltyAccount.TotalSpent);
            if (newTierId != loyaltyAccount.CurrentTierId)
            {
                loyaltyAccount.CurrentTierId = newTierId;
            }

            await _loyaltyAccountRepository.UpdateAsync(loyaltyAccount);
            return Unit.Value;

        }

        private async Task<Guid> CheckTierUpgrade(decimal totalSpent)
        {
            var memberShipTiers = await _membershipTierCache.GetOrSetAllAsync(async () =>
              {
                  // Gọi Repository để lấy dữ liệu (IEnumerable)
                  var data = await _membershipTierRepository.GetAllAsync();
                  // Chuyển đổi sang List để khớp với yêu cầu của hàm Cache
                  return data.ToList();
              });
            var newTier = memberShipTiers
                    .OrderByDescending(t => t.MinSpendRequired)
                    .FirstOrDefault(t => totalSpent >= t.MinSpendRequired);
            if (newTier == null) throw new DirectoryNotFoundException(MembershipTierMsg.NotFound);
            return newTier.Id;
        }
    }
}
