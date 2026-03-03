namespace SalesService.Application.Interfaces.Identity;

public interface IIdentityServiceClient
{
    Task<UserInfo?> GetUserByIdAsync(Guid userId);
}


public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public UserRole? Role { get; set; }
}

public class UserRole
{
    public Guid Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}
