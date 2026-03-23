using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Dtos.Campaign.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Campaign.Commands.RedeemCampaignVoucher
{
    public record class RedeemCampaignVoucherCommand(RedeemCampaignVoucherReq Request) : IRequest<CampaignRes>;
}
