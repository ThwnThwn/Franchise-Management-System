using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Dtos.MembershipTier
{
    public class MembershipTierRes
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public decimal MinSpendRequired { get; set; }

        public double DiscountRate { get; set; }
        public decimal AmountPerPoint { get; set; }

        public decimal ValuePerPoint { get; set; }
    }
}
