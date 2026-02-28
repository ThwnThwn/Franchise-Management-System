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
        public Guid OrderId { get; set; } = Guid.Empty;
    }

    public class GetByUserPhoneOrEmailRequest
    {
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    public class DeductPointRequest
    {
        public Guid Id { get; set; }
        public decimal PointToDeduct { get; set; } = 0;
        public Guid? OrderId { get; set; } = null;
    }
}