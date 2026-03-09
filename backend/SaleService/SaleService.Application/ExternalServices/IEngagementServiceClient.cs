using SalesService.Application.Dtos.Voucher;

namespace SalesService.Application.ExternalServices
{
    public interface IEngagementServiceClient
    {
        Task<IEnumerable<Voucher>> GetMyVoucherAsync();
    }
}
