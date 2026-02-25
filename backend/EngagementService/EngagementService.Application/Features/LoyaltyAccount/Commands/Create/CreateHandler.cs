using Confluent.Kafka;
using EngagementService.Application.Constants;
using EngagementService.Application.Helper;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Commands.Create
{

    public class CreateHandler : IRequestHandler<CreateCommand, Unit>
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IMembershipTierRepository _membershipTierRepository;
        private readonly IMembershipTierCache _membershipTierCache;
        public CreateHandler(ILoyaltyAccountRepository loyaltyAccountRepository, IMembershipTierRepository membershipTierRepository, IMembershipTierCache membershipTierCache)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _membershipTierRepository = membershipTierRepository;
            _membershipTierCache = membershipTierCache;
        }
        public async Task<Unit> Handle(CreateCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var defaultTier = await _membershipTierRepository.GetDefaultTierAsync();
            if(defaultTier == null)
            {
                throw new DirectoryNotFoundException(MembershipTierMsg.NotFound);
            }
            var point = await CalculatePointHelper.CalculatePoint(_membershipTierCache, null, request.OrderAmount);
            var loyaltyAccount = new Domain.Entities.LoyaltyAccount
            {
                Id = request.Id,
                AvailablePoints = point,
                TotalSpent = request.OrderAmount,
                CurrentTierId = defaultTier.Id,
            };
            await _loyaltyAccountRepository.AddAsync(loyaltyAccount);
            return Unit.Value;
        }
    }
}
