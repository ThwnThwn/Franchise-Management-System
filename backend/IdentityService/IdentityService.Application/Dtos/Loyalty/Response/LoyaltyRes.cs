using IdentityService.Domain.Commons;

namespace IdentityService.Application.Dtos.Loyalty.Response
{
    public class LoyaltyRes
    {
        public Guid Id { get; set; }

        public decimal AvailablePoints { get; set; }

        public decimal TotalSpent { get; set; }

        public virtual MembershipTier CurrentTier { get; set; } = null!;
    }

    public  class MembershipTier
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal MinSpendRequired { get; set; }
        public double DiscountRate { get; set; }
        public decimal AmountPerPoint { get; set; } // đổi từ tiền sang điểm
        public decimal ValuePerPoint { get; set; }  // đổi từ điểm sang tiền
    }
}
