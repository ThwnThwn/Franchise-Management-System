using EngagementService.Application.ClientInterface;
using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Application.Persistences.Repository;
using MediatR;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetById
{
    public class GetByPhoneOrEmailHandler : IRequestHandler<GetByPhoneOrEmailQuery, List<LoyaltyAccountDto>>
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IIdentityServiceClient _identityServiceClient;

        public GetByPhoneOrEmailHandler(ILoyaltyAccountRepository loyaltyAccountRepository, IIdentityServiceClient identityServiceClient)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _identityServiceClient = identityServiceClient;
        }

        public async Task<List<LoyaltyAccountDto>> Handle(GetByPhoneOrEmailQuery command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            //if(string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.PhoneNumber))
            //    throw new ArgumentException("Email hoặc số điện thoại không được để trống");

            var usersSimpleRep = new Dtos.Indentity.UserSimpleResp();
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                usersSimpleRep = await _identityServiceClient.GetUserIdByPhone(new Dtos.Indentity.GetUserIdByPhoneRequest { PhoneNumber = request.PhoneNumber });
            }
            else if (!string.IsNullOrEmpty(request.Email))
            {
                usersSimpleRep = await _identityServiceClient.GetUserIdByEmail(new Dtos.Indentity.GetUserIdByEmailRequest { Email = request.Email });
            }

            var loyaltyAccounts = await _loyaltyAccountRepository.GetUserByIdsAsync(usersSimpleRep.UserIDNameResp.Select(u => u.UserId).ToList());

            if (loyaltyAccounts == null || !loyaltyAccounts.Any())
                throw new DirectoryNotFoundException(LoyaltyAccountMsg.NotFound);

            var resList = new List<LoyaltyAccountDto>();

            foreach (var account in loyaltyAccounts)
            {
                if (account == null) continue;

                var userInfo = usersSimpleRep.UserIDNameResp.FirstOrDefault(u => u.UserId == account.Id);
                if (userInfo != null)
                {
                    resList.Add(new LoyaltyAccountDto
                    {
                        Id = account.Id,
                        UserName = userInfo.UserName,
                        CurrentTier = account.CurrentTier,
                        //PointLogs = account.PointLogs,
                        TotalSpent = account.TotalSpent,
                        AvailablePoints = account.AvailablePoints,
                        CurrentTierId = account.CurrentTierId
                    });
                }
            }
            return resList;
        }
    }
}