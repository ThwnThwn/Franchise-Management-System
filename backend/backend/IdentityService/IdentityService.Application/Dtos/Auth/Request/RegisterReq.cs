using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Dtos.Auth.Request;

public class RegisterReq
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = null!;

    [Required]
    [Phone]
    public string Phone { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Address { get; set; } = null!;

    [Required]
    [Range(0, 1, ErrorMessage = "Gender must be 0 or 1.")]
    public int Gender { get; set; }

    [Required]
    public DateTime DateOrBirth { get; set; }
}