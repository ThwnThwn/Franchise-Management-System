using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace EngagementService.Application.EventHandler;

public class RedeemCampaignVoucherEventHanlder(
    IVoucherRepository voucherRepository,
    ILoyaltyAccountRepository loyaltyAccountRepository,
    IEngagementUow uow
) : IIntegrationEventHandler<RedeemCampaignVoucherEvent>
{
    public async Task Handle(RedeemCampaignVoucherEvent @event)
    {
        try
        {
            await uow.BeginTransactionAsync();

            var voucher = await voucherRepository.GetByIdAsync(@event.VoucherId);
            var loyalty = await loyaltyAccountRepository.GetByUserIdAsync(@event.UserId);

            if (voucher == null || loyalty == null)
            {
                await uow.RollbackAsync();
                return;
            }

            var alreadyExists = loyalty.UserVouchers.Any(uv => uv.VoucherId == @event.VoucherId);
            if (alreadyExists)
            {
                await uow.RollbackAsync();
                return;
            }

            voucher.CurrentQuantity -= 1;

            loyalty.UserVouchers.Add(new UserVoucher
            {
                VoucherId = voucher.Id,
                UserId = loyalty.Id,
                AcquiredAt = DateTimeOffset.UtcNow,
            });

            await uow.SaveChangesAsync();
        }
        catch (Exception)
        {
            await uow.RollbackAsync();
            throw;
        }
    }
}