using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.MembershipTier.Commands.Update
{
    public class UpdateMembershiptierHandler : IRequestHandler<UpdateMembershipTierCommand, Unit>
    {
        private readonly IMembershipTierRepository _membershipTierRepository;
        private readonly IMembershipTierCache _membershipTierCache;
        public UpdateMembershiptierHandler(IMembershipTierRepository membershipTierRepository, IMembershipTierCache membershipTierCache)
        {
            _membershipTierRepository = membershipTierRepository;
            _membershipTierCache = membershipTierCache;
        }
        public async Task<Unit> Handle(UpdateMembershipTierCommand command
            , CancellationToken cancellationToken)
        {
            var request = command.Request;

            var tierToUpdate = await _membershipTierRepository.GetByIdAsync(request.Id);
            if (tierToUpdate == null) throw new DirectoryNotFoundException(MembershipTierMsg.NotFound);

            var allTiers = await _membershipTierCache.GetOrSetAllAsync(async () =>
            {
                // Gọi Repository để lấy dữ liệu (IEnumerable)
                var data = await _membershipTierRepository.GetAllAsync();

                // Chuyển đổi sang List để khớp với yêu cầu của hàm Cache
                return data.ToList();
            });

            //check để update
            if (!string.IsNullOrEmpty(request.Name))
            {
                if (allTiers.Any(t => t.Name.ToLower() == request.Name.ToLower() && t.Id != request.Id))
                    throw new InvalidOperationException(MembershipTierMsg.NameExisted);
                tierToUpdate.Name = request.Name;
            }

            if (request.MinSpendRequired.HasValue)
            {
                if (allTiers.Any(t => t.MinSpendRequired == request.MinSpendRequired.Value && t.Id != request.Id))
                    throw new InvalidOperationException(MembershipTierMsg.MinSpendRequiredExisted);
                tierToUpdate.MinSpendRequired = request.MinSpendRequired.Value;
            }

            if (request.DiscountRate.HasValue)
            {
                tierToUpdate.DiscountRate = request.DiscountRate.Value;
            }

            if (request.ValuePerPoint.HasValue)
            {
                tierToUpdate.ValuePerPoint = request.ValuePerPoint.Value;
            }

            if (request.AmountPerPoint.HasValue)
            {
                tierToUpdate.AmountPerPoint = request.AmountPerPoint.Value;
            }

            await _membershipTierRepository.UpdateAsync(tierToUpdate);
            await _membershipTierCache.ClearAllMembershipTierCacheAsync();

            return Unit.Value;
        }
    }
}
