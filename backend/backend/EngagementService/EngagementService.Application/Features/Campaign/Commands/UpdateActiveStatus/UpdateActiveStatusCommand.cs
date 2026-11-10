using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Dtos.Campaign.Response;
using MediatR;

namespace EngagementService.Application.Features.Campaign.Commands.UpdateActiveStatus
{
    public record UpdateActiveStatusCommand(UpdateActiveStatusReq Request) : IRequest<CampaignRes>;
}
