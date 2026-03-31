using EngagementService.Application.Dtos.LoyaltyAccount;
using MediatR;

namespace EngagementService.Application.Features.LoyaltyAccount.Commands.UpdatePoint;

public record class UpdatePointCommand(UpdatePointRequest Request) : IRequest<Unit>
{
}