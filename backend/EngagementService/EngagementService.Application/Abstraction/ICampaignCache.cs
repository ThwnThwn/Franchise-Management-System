using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Abstrations
{
    public interface ICampaignCache
    {
        Task<int> RedeemVoucherAsync(Guid voucherId, Guid userId);
        Task WarmUpVoucherAsync(Guid voucherId, int stock, DateTime endDate);
        Task DeleteVoucherCacheAsync(Guid voucherId);
    }
}
