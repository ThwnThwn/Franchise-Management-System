using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using EngagementService.Domain.Entities;
using MediatR;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EngagementService.Application.Features.LoyaltyAccount.Commands.RedeemVoucher
{
    public class RedeemVoucherHandler : IRequestHandler<RedeemVoucherCommand, Unit>
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IVoucherQueueRepository _voucherQueueRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly ILoyaltyUow _uow;
        private readonly IPointRuleCache _pointRuleCache;
        public RedeemVoucherHandler(ILoyaltyAccountRepository loyaltyAccountRepository, IVoucherQueueRepository voucherQueueRepository, IVoucherRepository voucherRepository, ILoyaltyUow uow, IPointRuleCache pointRuleCache)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _voucherQueueRepository = voucherQueueRepository;
            _voucherRepository = voucherRepository;
            _uow = uow;
            _pointRuleCache = pointRuleCache;
        }
        public async Task<Unit> Handle(RedeemVoucherCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var account = await _loyaltyAccountRepository.GetForUpdateAsync(request.LoyaltyAccountId, cancellationToken);

                var voucher = await _voucherRepository.GetForUpdateAsync(request.VoucherId, cancellationToken);

                await VerifyAccountAndVoucher(account, voucher, cancellationToken);

                var voucherQueueItem = new VoucherQueue
                {
                    UserId = request.LoyaltyAccountId,
                    VoucherId = request.VoucherId,
                };
                await _voucherQueueRepository.AddQueueAsync(voucherQueueItem, cancellationToken);

                await _loyaltyAccountRepository.RedeemVoucherAsync(_pointRuleCache, account!, voucher!, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                await _uow.RollbackAsync(cancellationToken);
                throw;
            }
            return Unit.Value;
        }

        private async Task VerifyAccountAndVoucher(Domain.Entities.LoyaltyAccount? account, Domain.Entities.Voucher? voucher, CancellationToken cancellationToken)
        {
            if (account == null)
                throw new ArgumentException(LoyaltyAccountMsg.NotFound);

            if (voucher == null)
                throw new ArgumentException(VoucherMsg.NotFound);

            if (!voucher.IsActive)
                throw new InvalidOperationException(VoucherMsg.InactiveVoucher);

            if (voucher.EndDate.HasValue && voucher.EndDate.Value < DateTimeOffset.UtcNow)
                throw new InvalidOperationException(VoucherMsg.ExpiredVoucher);

            if (voucher.MaxUsagePerUser.HasValue)
            {
                var usageCount = account.UserVouchers.Count(uv => uv.VoucherId == voucher.Id);
                if (usageCount >= voucher.MaxUsagePerUser.Value)
                    throw new InvalidOperationException(VoucherMsg.UsageLimitReached);
            }

            if (voucher.PointCost != null && account.AvailablePoints < voucher.PointCost)
                throw new InvalidOperationException(LoyaltyAccountMsg.InsufficientPoints);

            if (voucher.CurrentQuantity <= 0)
            {
                throw new InvalidOperationException(VoucherMsg.OutOfStock);
            }
            else
            {
                //lấy số lượng trong queue ra
                var amount = await _voucherQueueRepository.CountVoucherInQueue(voucher.Id, cancellationToken);
                if(amount != 0)
                {
                    if (voucher.CurrentQuantity - amount <= 0)
                    {
                        throw new InvalidOperationException(VoucherMsg.OutOfStock);
                    }
                }

            }
        }
    }
}
