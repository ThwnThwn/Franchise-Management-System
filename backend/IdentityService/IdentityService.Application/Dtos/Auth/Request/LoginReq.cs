using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Dtos.Auth.Request;

public class LoginReq
{
    [Required]
    public string EmailOrUsername { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;
}