using EngagementService.Application.Dtos.MembershipTier;
using MediatR;

namespace EngagementService.Application.Features.MembershipTier.Commands.Update
{
    public record class UpdateMembershipTierCommand(UpdateMembershipTierRequest Request) : IRequest<Unit>
    {
    }
}
