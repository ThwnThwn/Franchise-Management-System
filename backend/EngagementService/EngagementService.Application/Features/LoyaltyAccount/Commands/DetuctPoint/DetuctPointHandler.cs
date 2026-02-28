using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Commands.DetuctPoint
{
    public class DetuctPointHandler : IRequestHandler<DetuctPointCommand, Unit>
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;

        public DetuctPointHandler(ILoyaltyAccountRepository loyaltyAccountRepository)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
        }

        public async Task<Unit> Handle(DetuctPointCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var account = await _loyaltyAccountRepository.GetByIdAsync(request.Id);
            if (account == null)
                throw new DirectoryNotFoundException(LoyaltyAccountMsg.NotFound);

            if (account.AvailablePoints < request.PointToDetuct)
                throw new InvalidOperationException(LoyaltyAccountMsg.InsufficientPoints);

            account.AvailablePoints -= request.PointToDetuct;
            await _loyaltyAccountRepository.UpdateAsync(account);
            
            return Unit.Value;
        }
    }
}
