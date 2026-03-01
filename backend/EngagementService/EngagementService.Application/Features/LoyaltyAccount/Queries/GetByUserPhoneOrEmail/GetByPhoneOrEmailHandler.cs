using Confluent.Kafka;
using EngagementService.Application.ClientInterface;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetById
{
    public class GetByPhoneOrEmailHandler : IRequestHandler<GetByPhoneOrEmailQuery, List<Domain.Entities.LoyaltyAccount>>
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IIdentityServiceClient _identityServiceClient;
        public GetByPhoneOrEmailHandler(ILoyaltyAccountRepository loyaltyAccountRepository, IIdentityServiceClient identityServiceClient)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _identityServiceClient = identityServiceClient;
        }
        public async Task<List<Domain.Entities.LoyaltyAccount>> Handle(GetByPhoneOrEmailQuery command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            //if(string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.PhoneNumber))
            //    throw new ArgumentException("Email hoặc số điện thoại không được để trống");

            var usersIdRep = new Dtos.Indentity.UserIdsResp();
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                usersIdRep = await _identityServiceClient.GetUserIdByPhone(new Dtos.Indentity.GetUserIdByPhoneRequest { PhoneNumber = request.PhoneNumber });
            }
            else if (!string.IsNullOrEmpty(request.Email))
            {
                usersIdRep = await _identityServiceClient.GetUserIdByEmail(new Dtos.Indentity.GetUserIdByEmailRequest { Email = request.Email });
            }

            var loyaltyAccounts = await _loyaltyAccountRepository.GetUserByIdsAsync(usersIdRep.UserIds);
            //var loyaltyAccount = await _loyaltyAccountRepository.GetByUserIdAsync(Guid.Parse("00964234-2ce1-4bbd-b926-64bf68fef73b"));

            if(loyaltyAccounts == null || !loyaltyAccounts.Any())
                throw new DirectoryNotFoundException("Không tìm thấy tài khoản");

            return loyaltyAccounts.ToList();
        }
    }
}
