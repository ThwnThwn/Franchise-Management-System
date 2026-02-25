using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;

namespace EngagementService.Application.Helper
{
    public static class CalculatePointHelper
    {
        public static async Task<decimal> CalculatePoint(IMembershipTierRepository _membershipTierRepository, IMembershipTierCache _membershipTierCache, Guid? tierId, decimal orderAmount)
        {
            var allTiers = await _membershipTierCache.GetOrSetAllAsync(async () =>
            {
                // Gọi Repository để lấy dữ liệu (IEnumerable)
                var data = await _membershipTierRepository.GetAllAsync();

                // Chuyển đổi sang List để khớp với yêu cầu của hàm Cache
                return data.ToList();
            });

            //lấy default tier
            var tier = allTiers
                    .OrderBy(t => t.MinSpendRequired)
                    .FirstOrDefault();
            if (tier == null) throw new DirectoryNotFoundException(MembershipTierMsg.NotFound);

            if (tierId != null)
            {
                tier = await _membershipTierCache.GetMembershipTierAsync((Guid)tierId);
                if (tier == null) throw new DirectoryNotFoundException(MembershipTierMsg.NotFound);
            }
            return Math.Floor(orderAmount / tier.AmountPerPoint);
        }
    }
}
