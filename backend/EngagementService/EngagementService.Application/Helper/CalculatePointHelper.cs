using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Helper
{
    public static class CalculatePointHelper
    {
        public static async Task<decimal> CalculatePoint(IMembershipTierCache _membershipTierCache, Guid? tierId, decimal orderAmount)
        {
            double rate = 1.0;
            if (tierId != null)
            {
                var tier = await _membershipTierCache.GetMembershipTierAsync((Guid)tierId);
                if (tier == null) throw new DirectoryNotFoundException(MembershipTierMsg.NotFound);
                rate = tier.EarningRate;
            }
            return Math.Floor(orderAmount / 10000 * (decimal)rate);
        }
    }
}
