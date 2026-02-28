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
            PointReasonCode reasonCode, PointActionType type, string description)
        {
            var pointLog = new Domain.Entities.PointLog
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
                User = null,
            };
            await _repo.AddAsync(pointLog);
        }
    }
}
