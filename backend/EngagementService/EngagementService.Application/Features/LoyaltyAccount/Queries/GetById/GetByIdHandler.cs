using Confluent.Kafka;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetById
{
    public class GetByIdHandler : IRequestHandler<GetByIdQuery, Domain.Entities.LoyaltyAccount?>
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        public GetByIdHandler(ILoyaltyAccountRepository loyaltyAccountRepository)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
        }
        public async Task<Domain.Entities.LoyaltyAccount?> Handle(GetByIdQuery request, CancellationToken cancellationToken)
        {
            var loyaltyAccount = await _loyaltyAccountRepository.GetByIdAsync(request.Id);
            return loyaltyAccount;
        }
    }
}
