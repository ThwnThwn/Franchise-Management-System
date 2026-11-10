using EngagementService.Application.Dtos.LoyaltyAccount;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Commands.Create
{
    public record class CreateCommand(UpSertReqest Request) : IRequest<Unit>
    {
    }
}
