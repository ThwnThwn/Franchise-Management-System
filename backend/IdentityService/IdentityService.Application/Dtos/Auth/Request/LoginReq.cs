using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Dtos.Auth.Request;

public class LoginReq
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;
}
