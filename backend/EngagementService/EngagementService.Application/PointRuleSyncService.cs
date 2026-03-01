using EngagementService.Application.Abstraction;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application
{
    public class PointRuleSyncService : IPointRuleSyncService
    {
        private readonly IPointRuleRepository _pointRuleRepository;
        private readonly IPointRuleCache _pointRuleCache;

        public PointRuleSyncService(
            IPointRuleRepository pointRuleRepository,
            IPointRuleCache pointRuleCache  )
        {
            _pointRuleRepository = pointRuleRepository;
            _pointRuleCache = pointRuleCache;
        }
        public async Task SyncAllAsync()
        {
            // 1. Dọn dẹp sạch cache cũ để tránh dữ liệu rác/lệch
            await _pointRuleCache.ClearAllPointRuleCacheAsync();

            // 2. Lấy toàn bộ danh sách Rule từ Database
            var allRules = await _pointRuleRepository.GetAllAsync();

            if (allRules == null || !allRules.Any())
            {
                return;
            }

            // 3. Đẩy từng Rule lên Redis
            // Bạn có thể dùng Task.WhenAll để tăng tốc độ nếu danh sách lớn
            var syncTasks = allRules.Select(rule =>
                _pointRuleCache.SetPointRuleAsync(rule.Id, rule));

            await Task.WhenAll(syncTasks);
        }
    }
}
