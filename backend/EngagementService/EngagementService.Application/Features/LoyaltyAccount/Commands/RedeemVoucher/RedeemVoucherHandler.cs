using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using MediatR;
using System.Runtime.CompilerServices;

namespace EngagementService.Application.Features.LoyaltyAccount.Commands.RedeemVoucher
{
    public class RedeemVoucherHandler : IRequestHandler<RedeemVoucherCommand, Unit>
    {
        private readonly ILoyaltyAccountUow _uow;
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IPointRuleCache _pointRuleCache;
        public RedeemVoucherHandler(ILoyaltyAccountUow uow, ILoyaltyAccountRepository loyaltyAccountRepository, IPointRuleCache pointRuleCache)
        {
            _uow = uow;
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _pointRuleCache = pointRuleCache;
        }
        public async Task<Unit> Handle(RedeemVoucherCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                await _loyaltyAccountRepository.RedeemVoucherAsync(_pointRuleCache, request.LoyaltyAccountId, request.VoucherId, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
            }
            catch(Exception)
            {
                await _uow.RollbackAsync(cancellationToken);
                throw;
            }

            return Unit.Value;
        }
    }
}
