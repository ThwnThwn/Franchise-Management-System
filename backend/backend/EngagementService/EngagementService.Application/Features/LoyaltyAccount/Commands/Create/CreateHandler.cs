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
        private readonly IPointRuleCache _pointRuleCache;
        private readonly IPointRuleRepository _pointRuleRepository;

        public CreateHandler(ILoyaltyAccountRepository loyaltyAccountRepository, IMembershipTierRepository membershipTierRepository, IMembershipTierCache membershipTierCache, IPointLogRepository pointLogRepository, IPointRuleCache pointRuleCache, IPointRuleRepository pointRuleRepository)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _membershipTierRepository = membershipTierRepository;
            _membershipTierCache = membershipTierCache;
            _pointLogRepository = pointLogRepository;
            _pointRuleCache = pointRuleCache;
            _pointRuleRepository = pointRuleRepository;
        }

        public async Task<Unit> Handle(CreateCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            if ((await _loyaltyAccountRepository.GetByIdAsync(request.Id)) != null)
                throw new InvalidOperationException(LoyaltyAccountMsg.AlreadyExists);

            var defaultTier = await _membershipTierRepository.GetDefaultTierAsync();
            if (defaultTier == null)
                throw new DirectoryNotFoundException(MembershipTierMsg.NotFound);

            var point = await CalculatePointHelper.CalculatePoint(_membershipTierRepository, _membershipTierCache, null, request.OrderAmount);

            var loyaltyAccount = new Domain.Entities.LoyaltyAccount
            {
                Id = request.Id,
                AvailablePoints = point,
                TotalSpent = request.OrderAmount,
                CurrentTierId = defaultTier.Id,
            };

            if (point > 0)
            {
                //sinh point log
                var allRules = await _pointRuleCache.GetOrSetAllAsync(async () =>
                {
                    // Gọi Repository để lấy dữ liệu (IEnumerable)
                    var data = await _pointRuleRepository.GetAllAsync();
                    // Chuyển đổi sang List để khớp với yêu cầu của hàm Cache
                    return data.ToList();
                });

                var pointLog = HandlePointLog.CreatePointLog(loyaltyAccount, 0, loyaltyAccount.AvailablePoints, point, request.OrderId, PointReasonCode.ORDER_COMPLETED, PointActionType.EARN, "Điểm khởi tạo", allRules);

                loyaltyAccount.PointLogs.Add(pointLog);
            }

            await _loyaltyAccountRepository.AddAsync(loyaltyAccount);

            //có điểm ms ghi log

            return Unit.Value;
        }
    }
}