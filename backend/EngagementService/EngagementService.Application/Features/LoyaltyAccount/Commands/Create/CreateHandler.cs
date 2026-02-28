using Confluent.Kafka;
using EngagementService.Application.Constants;
using EngagementService.Application.Helper;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Commands.Create
{

    public class CreateHandler : IRequestHandler<CreateCommand, Unit>
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IMembershipTierRepository _membershipTierRepository;
        private readonly IMembershipTierCache _membershipTierCache;
        private readonly IPointLogRepository _pointLogRepository;
        public CreateHandler(ILoyaltyAccountRepository loyaltyAccountRepository, IMembershipTierRepository membershipTierRepository, IMembershipTierCache membershipTierCache, IPointLogRepository pointLogRepository)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _membershipTierRepository = membershipTierRepository;
            _membershipTierCache = membershipTierCache;
            _pointLogRepository = pointLogRepository;
        }
        public async Task<Unit> Handle(CreateCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var defaultTier = await _membershipTierRepository.GetDefaultTierAsync();
            if(defaultTier == null)
                throw new DirectoryNotFoundException(MembershipTierMsg.NotFound);
            
            var point = await CalculatePointHelper.CalculatePoint(_membershipTierRepository, _membershipTierCache, null, request.OrderAmount);
            
            var loyaltyAccount = new Domain.Entities.LoyaltyAccount
            {
                Id = request.Id,
                AvailablePoints = point,
                TotalSpent = request.OrderAmount,
                CurrentTierId = defaultTier.Id,
            };

            await _loyaltyAccountRepository.AddAsync(loyaltyAccount);

            //có điểm ms ghi log
            if (point > 0)
                await HandlePointLog.CreatePointLog(_pointLogRepository, loyaltyAccount, 0, loyaltyAccount.AvailablePoints, point, request.OrderId, PointReasonCode.ORDER_COMPLETED, PointActionType.EARN, "Initial points");
            return Unit.Value;
        }
    }
}
