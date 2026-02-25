using EngagementService.Application.Dtos.MembershipTier;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.MembershipTier.Queries.GetAll
{
    public class GetAllMembershipTierHandler : IRequestHandler<GetAllMembershipTierQuery, List<MembershipTierRes>>
    {
        private readonly IMembershipTierRepository _membershipTierRepository;
        private readonly IMembershipTierCache _membershipTierCache;

        public GetAllMembershipTierHandler(IMembershipTierRepository membershipTierRepository, IMembershipTierCache membershipTierCache)
        {
            _membershipTierRepository = membershipTierRepository;
            _membershipTierCache = membershipTierCache;
        }

        public async Task<List<MembershipTierRes>> Handle(GetAllMembershipTierQuery request, CancellationToken cancellationToken)
        {
            var allTiers = await _membershipTierCache.GetOrSetAllAsync(async () =>
            {
                // Gọi Repository để lấy dữ liệu (IEnumerable)
                var data = await _membershipTierRepository.GetAllAsync();

                // Chuyển đổi sang List để khớp với yêu cầu của hàm Cache
                return data.ToList();
            });

            if(allTiers == null || allTiers.Count == 0)
                throw new DirectoryNotFoundException("Không tìm thấy hạng thành viên nào");

            return allTiers.Select(t => new MembershipTierRes
            {
                Id = t.Id,
                Name = t.Name,
                MinSpendRequired = t.MinSpendRequired,
                DiscountRate = t.DiscountRate,
                ValuePerPoint = t.ValuePerPoint,
                AmountPerPoint = t.AmountPerPoint
            }).ToList();
        }
    }
}
