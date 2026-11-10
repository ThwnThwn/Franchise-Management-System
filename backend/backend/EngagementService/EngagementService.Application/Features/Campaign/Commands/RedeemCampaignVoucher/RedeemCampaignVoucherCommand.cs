using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Dtos.Campaign.Response;
using MediatR;

namespace EngagementService.Application.Features.Campaign.Commands.RedeemCampaignVoucher
{
    public record class RedeemCampaignVoucherCommand(RedeemCampaignVoucherReq Request) : IRequest<CampaignRes>;
}
