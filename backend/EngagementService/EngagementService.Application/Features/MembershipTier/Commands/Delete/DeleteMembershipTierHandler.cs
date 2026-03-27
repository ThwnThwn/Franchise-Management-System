using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.MembershipTier.Commands.Delete
{
    public class DeleteMembershipTierHandler : IRequestHandler<DeleteMembershipTierCommand, Unit>
    {
        private readonly IMembershipTierRepository _membershipTierRepository;
        private readonly IMembershipTierCache _membershipTierCache;

        public DeleteMembershipTierHandler(IMembershipTierRepository membershipTierRepository, IMembershipTierCache membershipTierCache)
        {
            _membershipTierRepository = membershipTierRepository;
            _membershipTierCache = membershipTierCache;
        }
        public async Task<Unit> Handle(DeleteMembershipTierCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
            {
                throw new ArgumentException(CommonMsg.InvalidId);
            }
            await _membershipTierRepository.DeleteAsync(request.Id);
            // Xóa cache cũ để buộc lần gọi sau phải lấy dữ liệu mới từ DB
            await _membershipTierCache.ClearAllMembershipTierCacheAsync();
            return Unit.Value;
        }
    }
}
