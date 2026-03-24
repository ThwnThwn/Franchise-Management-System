namespace SalesService.Application.Interfaces.Identity;

public interface IIdentityServiceClient
{
    Task<UserInfo?> GetUserByIdAsync(Guid userId);
    Task<List<UserInfo>> GetUsersByIdsAsync(List<Guid> userIds);
}


public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public UserRole? Role { get; set; }
    public LoyaltyRes? LoyaltyTier { get; set; }
}

public class UserRole
{
    public Guid Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class LoyaltyRes
{
    public Guid Id { get; set; }

    public decimal AvailablePoints { get; set; }

    public decimal TotalSpent { get; set; }

    public virtual MembershipTier? CurrentTier { get; set; } = null!;
}

public class MembershipTier
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal MinSpendRequired { get; set; }
    public double DiscountRate { get; set; }
    public decimal AmountPerPoint { get; set; } // đổi từ tiền sang điểm
    public decimal ValuePerPoint { get; set; }  // đổi từ điểm sang tiền
}
