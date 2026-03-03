using EngagementService.Application.Constants;
using EngagementService.Application.Helper;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Commands.DetuctPoint
{
    public class DeductPointHandler : IRequestHandler<DeductPointCommand, Unit>
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IPointLogRepository _pointLogRepository;
        private readonly IPointRuleCache _pointRuleCache;
        private readonly IPointRuleRepository _pointRuleRepository;

        public DeductPointHandler(ILoyaltyAccountRepository loyaltyAccountRepository, IPointLogRepository pointLogRepository, IPointRuleCache pointRuleCache, IPointRuleRepository pointRuleRepository)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _pointLogRepository = pointLogRepository;
            _pointRuleCache = pointRuleCache;
            _pointRuleRepository = pointRuleRepository;
        }

        public async Task<Unit> Handle(DeductPointCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var account = await _loyaltyAccountRepository.GetByIdAsync(request.Id);
            if (account == null)
                throw new DirectoryNotFoundException(LoyaltyAccountMsg.NotFound);

            if (account.AvailablePoints < request.PointToDeduct)
                throw new InvalidOperationException(LoyaltyAccountMsg.InsufficientPoints);

            account.AvailablePoints -= request.PointToDeduct;
            await _loyaltyAccountRepository.UpdateAsync(account);

            var allRules = await _pointRuleCache.GetOrSetAllAsync(async () =>
            {
                // Gọi Repository để lấy dữ liệu (IEnumerable)
                var data = await _pointRuleRepository.GetAllAsync();

                // Chuyển đổi sang List để khớp với yêu cầu của hàm Cache
                return data.ToList();
            });
            await HandlePointLog.CreatePointLog(_pointLogRepository, account, account.AvailablePoints + request.PointToDeduct, account.AvailablePoints, request.PointToDeduct, request.OrderId, PointReasonCode.POINT_REDEEM, PointActionType.REDEEM, "Points deducted", allRules);

            return Unit.Value;
        }
    }
}
