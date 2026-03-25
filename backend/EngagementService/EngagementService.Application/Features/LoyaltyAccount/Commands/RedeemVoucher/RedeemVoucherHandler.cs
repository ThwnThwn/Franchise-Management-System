using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Commands.RedeemVoucher;

public class RedeemVoucherHandler(ILoyaltyAccountRepository loyaltyAccountRepository,
    IVoucherQueueRepository voucherQueueRepository,
    IVoucherRepository voucherRepository,
    IEngagementUow uow,
    IPointRuleCache pointRuleCache
) : IRequestHandler<RedeemVoucherCommand, Unit>
{
    public async Task<Unit> Handle(RedeemVoucherCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        await uow.BeginTransactionAsync(cancellationToken);
        try
        {
            var account = await loyaltyAccountRepository.GetForUpdateAsync(request.LoyaltyAccountId, cancellationToken);

            var voucher = await voucherRepository.GetForUpdateAsync(request.VoucherId, cancellationToken);

            await VerifyAccountAndVoucher(account, voucher, cancellationToken);

            var voucherQueueItem = new VoucherQueue
            {
                UserId = request.LoyaltyAccountId,
                VoucherId = request.VoucherId,
            };
            await voucherQueueRepository.AddQueueAsync(voucherQueueItem, cancellationToken);

            await loyaltyAccountRepository.RedeemVoucherAsync(pointRuleCache, account!, voucher!, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            await uow.RollbackAsync(cancellationToken);
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
            var amount = await voucherQueueRepository.CountVoucherInQueue(voucher.Id, cancellationToken);
            if (amount != 0)
            {
                if (voucher.CurrentQuantity - amount <= 0)
                {
                    throw new InvalidOperationException(VoucherMsg.OutOfStock);
                }
            }
        }
    }
}