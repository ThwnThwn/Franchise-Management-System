namespace IdentityService.Application.Abstrations
{
    public interface ICampaignCache
    {
        Task<int> RedeemVoucherAsync(Guid voucherId, Guid userId);

        Task WarmUpVoucherAsync(Guid voucherId, int stock, DateTimeOffset endDate);

        Task DeleteVoucherCacheAsync(Guid voucherId);
    }
}