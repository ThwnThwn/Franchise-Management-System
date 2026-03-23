namespace EngagementService.Application.Dtos.LoyaltyAccount
{
    public class LoyaltyReportDto
    {
        public int TotalMembers { get; set; }
        public decimal TotalPointsEarned { get; set; }
        public decimal TotalPointsRedeemed { get; set; }
        public List<TierDistributionDto> TierDistribution { get; set; } = new();
    }

    public class TierDistributionDto
    {
        public string TierName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public decimal MinSpend { get; set; }
    }

    public class LoyaltyAccountDto
    {
        public Guid Id { get; set; }

        public decimal AvailablePoints { get; set; }

        public decimal TotalSpent { get; set; }

        public Guid CurrentTierId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public virtual Domain.Entities.MembershipTier CurrentTier { get; set; } = null!;

        //public virtual ICollection<PointLog> PointLogs { get; set; } = new List<PointLog>();
    }
}