using Confluent.Kafka;
using EngagementService.Application.ClientInterface;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetById
{
    public class GetByPhoneOrEmailHandler : IRequestHandler<GetByPhoneOrEmailQuery, Domain.Entities.LoyaltyAccount?>
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IIdentityServiceClient _identityServiceClient;
        public GetByPhoneOrEmailHandler(ILoyaltyAccountRepository loyaltyAccountRepository, IIdentityServiceClient identityServiceClient)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _identityServiceClient = identityServiceClient;
        }
        public async Task<Domain.Entities.LoyaltyAccount?> Handle(GetByPhoneOrEmailQuery command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            if(string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.PhoneNumber))
                throw new Exception("Email hoặc số điện thoại không được để trống");

            var userIdRep = new Dtos.Indentity.UserIdRep();
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                userIdRep = await _identityServiceClient.GetUserIdByPhone(new Dtos.Indentity.GetUserIdByPhoneRequest { PhoneNumber = request.PhoneNumber });
            }
            else if (!string.IsNullOrEmpty(request.Email))
            {
                userIdRep = await _identityServiceClient.GetUserIdByEmail(new Dtos.Indentity.GetUserIdByEmailRequest { Email = request.Email });
            }

            var loyaltyAccount = await _loyaltyAccountRepository.GetByUserIdAsync(userIdRep.UserId);
            //var loyaltyAccount = await _loyaltyAccountRepository.GetByUserIdAsync(Guid.Parse("00964234-2ce1-4bbd-b926-64bf68fef73b"));

            return loyaltyAccount;
        }
    }
}
