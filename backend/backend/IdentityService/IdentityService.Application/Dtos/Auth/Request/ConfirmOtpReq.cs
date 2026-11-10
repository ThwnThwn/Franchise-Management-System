using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Dtos.Auth.Request;

public class ConfirmOtpReq
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Length(6, 6)]
    public string Otp { get; set; } = null!;
}
