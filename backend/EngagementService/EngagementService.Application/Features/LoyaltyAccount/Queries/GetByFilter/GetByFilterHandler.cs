using MediatR;
using EngagementService.Application.Persistences.Repository;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetByIdsFilter
{
    public class GetByFilterHandler
        : IRequestHandler<GetByFilterQuery, IEnumerable<Domain.Entities.LoyaltyAccount>>
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;

        public GetByFilterHandler(ILoyaltyAccountRepository loyaltyAccountRepository)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
        }

        public async Task<IEnumerable<Domain.Entities.LoyaltyAccount>> Handle(
            GetByFilterQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _loyaltyAccountRepository.GetByFilterHandler(
                request.TierId,
                request.StartAt,
                request.EndAt);

            return result!;
        }
    }
}