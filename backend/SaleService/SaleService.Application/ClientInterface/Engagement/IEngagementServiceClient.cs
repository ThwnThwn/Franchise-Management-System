using SalesService.Application.Dtos.Voucher;

namespace SalesService.Application.ClientInterface.Engagement;

public interface IEngagementServiceClient
{
    Task<IEnumerable<Voucher>> GetMyVoucherAsync(Guid userId);

    Task<decimal> GetMyPointByEmailAsync(string email);
}