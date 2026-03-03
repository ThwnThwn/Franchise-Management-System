using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using EngagementService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Helper
{
    public static class HandlePointLog
    {
        public async static Task CreatePointLog(IPointLogRepository _repo,
            LoyaltyAccount account
            , decimal pointBefore, decimal pointAfter, decimal points, Guid? orderId,
            PointReasonCode reasonCode, PointActionType type, string description, List<PointRule> pointRuleCache)
        {
            var rule = pointRuleCache.FirstOrDefault(r => r.ReasonCode == reasonCode);
            int expiredDays = rule != null ? rule.ExpiryDays : 180;

            var pointLog = new PointLog
            {
                Id = Guid.NewGuid(),
                UserId = account.Id,
                Amount = points,
                BalanceBefore = pointBefore,
                BalanceAfter = pointAfter,
                ActionType = type,
                Description = description,
                ReasonCode = reasonCode,
                ReferenceId = orderId,
                ExpiredAt = DateTime.UtcNow.AddDays(expiredDays),
                User = account,
            };
            await _repo.AddAsync(pointLog);
        }
    }
}
