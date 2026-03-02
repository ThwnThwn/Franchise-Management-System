using EngagementService.Application.Persistences.Repository;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetByIdsFilter
{
    public class GetByIdsFilterHandler
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        public GetByIdsFilterHandler(ILoyaltyAccountRepository loyaltyAccountRepository)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
        }
        public async Task<IEnumerable<Domain.Entities.LoyaltyAccount>> Handle(GetByIdsFilterQuery request, CancellationToken cancellationToken)
        {

            var result = await _loyaltyAccountRepository
                .GetByIdsFilterHandler(
                    request.UserIds,
                    request.TierId,
                    request.StartAt,
                    request.EndAt);

            return result;
        }
    }
}
