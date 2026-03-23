using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Dtos.Campaign.Request
{
    public class RedeemCampaignVoucherReq
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public Guid VoucherId {  get; set; }= Guid.Empty;
    }
}
