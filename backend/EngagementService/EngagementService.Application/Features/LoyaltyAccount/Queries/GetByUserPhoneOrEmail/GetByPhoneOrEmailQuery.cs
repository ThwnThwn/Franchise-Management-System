using EngagementService.Application.Dtos.LoyaltyAccount;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetById
{
    public record class GetByPhoneOrEmailQuery(GetByUserPhoneOrEmailRequest Request) : IRequest<List<LoyaltyAccountDto>>
    {
    }
}
