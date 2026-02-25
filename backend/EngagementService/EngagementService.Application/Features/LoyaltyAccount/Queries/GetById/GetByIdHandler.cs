using MediatR;
using EngagementService.Domain.Entities;
using EngagementService.Application.Persistences.Repository;
namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetById
{
    public class GetByIdHandler : IRequestHandler<GetByIdQuery, Domain.Entities.LoyaltyAccount?>
    {
        private readonly ILoyaltyAccountRepository loyaltyAccountRepository;
        public GetByIdHandler(ILoyaltyAccountRepository loyaltyAccountRepository)
        {
            this.loyaltyAccountRepository = loyaltyAccountRepository;
        }
        public async Task<Domain.Entities.LoyaltyAccount?> Handle(GetByIdQuery request, CancellationToken cancellationToken)
        {
            return await loyaltyAccountRepository.GetByIdAsync(request.Id);
        }
    }
}
