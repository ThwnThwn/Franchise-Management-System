using MediatR;

namespace EngagementService.Application.Features.MembershipTier.Commands.Delete
{
    public record class DeleteMembershipTierCommand(Guid Id) : IRequest<Unit>
    {
    }
}
