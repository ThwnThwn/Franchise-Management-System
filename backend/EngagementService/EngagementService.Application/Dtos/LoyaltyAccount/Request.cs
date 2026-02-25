using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Dtos.LoyaltyAccount
{
    public class UpSertReqest
    {
        public Guid Id { get; set; }
        public decimal OrderAmount { get; set; }
    }

    public class GetByUserPhoneOrEmailRequest
    {
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}