using EngagementService.Application.AppExceptions;
using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.MembershipTier.Commands.Create
{
    public class CreateMembershipTierHandler : IRequestHandler<CreateMembershipTierCommand, Unit>
    {
        private readonly IMembershipTierRepository _membershipTierRepository;
        private readonly IMembershipTierCache _membershipTierCache;
        public CreateMembershipTierHandler(IMembershipTierRepository membershipTierRepository, IMembershipTierCache membershipTierCache)
        {
            _membershipTierRepository = membershipTierRepository;
            _membershipTierCache = membershipTierCache;
        }
        public async Task<Unit> Handle(CreateMembershipTierCommand request, CancellationToken cancellationToken)
        {
            var allTiers = await _membershipTierCache.GetOrSetAllAsync(async () =>
            {
                // Gọi Repository để lấy dữ liệu (IEnumerable)
                var data = await _membershipTierRepository.GetAllAsync();

                // Chuyển đổi sang List để khớp với yêu cầu của hàm Cache
                return data.ToList();
            });

            //Ktra xem tên đã tồn tại chưa
            if (allTiers.Any(t => t.Name.ToLower() == request.Request.Name.ToLower()))
                throw new ConflictDuplicateException(MembershipTierMsg.NameExisted);

            //Ktra xem min spend đã tồn tại chưa
            if (allTiers.Any(t => t.MinSpendRequired == request.Request.MinSpendRequired))
                throw new ConflictDuplicateException(MembershipTierMsg.MinSpendRequiredExisted);

            var membershipTier = new Domain.Entities.MembershipTier
            {
                Id = Guid.NewGuid(),
                Name = request.Request.Name,
                MinSpendRequired = request.Request.MinSpendRequired,
                DiscountRate = request.Request.DiscountRate,
                ValuePerPoint = request.Request.ValuePerPoint,
                AmountPerPoint = request.Request.AmountPerPoint
            };
            await _membershipTierRepository.AddAsync(membershipTier);

            // Xóa cache cũ để buộc lần gọi sau phải lấy dữ liệu mới từ DB
            await _membershipTierCache.ClearAllMembershipTierCacheAsync();

            return Unit.Value;
        }
    }
}
