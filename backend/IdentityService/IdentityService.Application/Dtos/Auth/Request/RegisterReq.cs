using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Dtos.Auth.Request;

public class RegisterReq
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [Required]
    [Phone]
    public string Phone { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Address { get; set; } = null!;
}