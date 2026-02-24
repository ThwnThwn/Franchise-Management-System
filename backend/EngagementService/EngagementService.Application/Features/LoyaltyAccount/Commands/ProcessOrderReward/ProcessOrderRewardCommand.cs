using EngagementService.Application.Dtos.LoyaltyAccount;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Commands.ProcessOrderReward
{
    public record class ProcessOrderRewardCommand(UpSertReqest Request) : IRequest<Unit>
    {
    }
}
