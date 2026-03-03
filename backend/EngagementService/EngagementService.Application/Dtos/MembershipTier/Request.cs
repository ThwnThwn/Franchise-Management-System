using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Dtos.MembershipTier
{
    public class CreateMembershipTierRequest
    {
        public string Name { get; set; } = null!;

        public decimal MinSpendRequired { get; set; }

        public double DiscountRate { get; set; }

        public decimal AmountPerPoint { get; set; }

        public decimal ValuePerPoint { get; set; }
    }

    public class UpdateMembershipTierRequest
    {
        public Guid Id { get; set; }
        
        public string? Name { get; set; } = null;

        public decimal? MinSpendRequired { get; set; } = null;

        public double? DiscountRate { get; set; } = null;

        public decimal? AmountPerPoint { get; set; } = null;

        public decimal? ValuePerPoint { get; set; } = null;
    }
}
