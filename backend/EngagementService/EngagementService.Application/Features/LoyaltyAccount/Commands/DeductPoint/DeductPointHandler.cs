using EngagementService.Application.Constants;
using EngagementService.Application.Helper;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Commands.DetuctPoint
{
    public class DeductPointHandler : IRequestHandler<DeductPointCommand, Unit>
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IPointLogRepository _pointLogRepository;

        public DeductPointHandler(ILoyaltyAccountRepository loyaltyAccountRepository, IPointLogRepository pointLogRepository)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _pointLogRepository = pointLogRepository;
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

            await HandlePointLog.CreatePointLog(_pointLogRepository, account, account.AvailablePoints + request.PointToDeduct, account.AvailablePoints, request.PointToDeduct, request.OrderId, PointReasonCode.POINT_REDEEM, PointActionType.REDEEM, "Points deducted for order reward");

            return Unit.Value;
        }
    }
}
