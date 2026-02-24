using EngagementService.Application.Abstraction;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;

namespace EngagementService.Application
{
    public class MembershipTierSyncService : IMembershipTierSyncService
    {
        private readonly IMembershipTierRepository _membershipTierRepository;
        private readonly IMembershipTierCache _membershipTierCache;

        public MembershipTierSyncService(
            IMembershipTierRepository membershipTierRepository,
            IMembershipTierCache membershipTierCache)
        {
            _membershipTierRepository = membershipTierRepository;
            _membershipTierCache = membershipTierCache;
        }

        public async Task SyncAllAsync()
        {
            // 1. Dọn dẹp sạch cache cũ để tránh dữ liệu rác/lệch
            await _membershipTierCache.ClearAllMembershipTierCacheAsync();

            // 2. Lấy toàn bộ danh sách Tier từ Database
            var allTiers = await _membershipTierRepository.GetAllAsync();

            if (allTiers == null || !allTiers.Any())
            {
                return;
            }

            // 3. Đẩy từng Tier lên Redis
            // Bạn có thể dùng Task.WhenAll để tăng tốc độ nếu danh sách lớn
            var syncTasks = allTiers.Select(tier =>
                _membershipTierCache.SetMembershipTierAsync(tier.Id, tier));

            await Task.WhenAll(syncTasks);
        }
    }
}

