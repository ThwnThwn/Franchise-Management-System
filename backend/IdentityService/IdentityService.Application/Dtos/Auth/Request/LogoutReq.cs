using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Dtos.Auth.Request;

public class LogoutReq
{
    [Required]
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// Access token để blacklist (optional - sẽ lấy từ Authorization header nếu không có)
    /// </summary>
    public string? AccessToken { get; set; }
}

