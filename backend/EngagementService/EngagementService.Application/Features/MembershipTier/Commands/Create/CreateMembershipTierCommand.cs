using EngagementService.Application.Dtos.MembershipTier;
using MediatR;

namespace EngagementService.Application.Features.MembershipTier.Commands.Create
{
    public record class CreateMembershipTierCommand(CreateMembershipTierRequest Request) : IRequest<Unit>
    {
    }
}
