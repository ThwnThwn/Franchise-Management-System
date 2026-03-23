using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Events
{
    public class RedeemCampaignVoucherEvent
    {
        public Guid UserId {  get; set; }
        public Guid VoucherId { get; set; }
    }
}
