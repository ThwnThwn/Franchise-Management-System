using EngagementService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Persistences.Cache
{
    public interface IMembershipTierCache
    {
        Task SetMembershipTierAsync(Guid tierId, MembershipTier data);
        Task<MembershipTier?> GetMembershipTierAsync(Guid tierId);
        Task<bool> IsMembershipTierLoadedAsync(Guid tierId);
        Task RemoveMembershipTierAsync(Guid tierId);
        Task ClearAllMembershipTierCacheAsync();
        Task<List<MembershipTier>> GetOrSetAllAsync(Func<Task<List<MembershipTier>>> getAllFromDb);
    }
}
