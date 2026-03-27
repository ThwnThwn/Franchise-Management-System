using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Dtos.Campaign.Response;
using MediatR;

namespace EngagementService.Application.Features.Campaign.Commands.Create
{
    public record CreateCampaginCommand(CreateCampaginReq Request) : IRequest<CampaignRes>;
}
