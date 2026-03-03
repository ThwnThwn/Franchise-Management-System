using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Features.Users.Commands.AdminCreateUser;

public record AdminCreateUserCommand(AdminCreateUserReq Request) : IRequest<UserRep>;

public class AdminCreateUserReq
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;

    [MaxLength(50)]
    public string? LastName { get; set; }

    [MaxLength(100)]
    public string? Username { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [Required]
    [Range(0, 1, ErrorMessage = "Gender must be 0 or 1.")]
    public int Gender { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public string RoleSlug { get; set; } = null!;

    public decimal? CurrentHourlyWage { get; set; }
    public Guid? HomeStoreId { get; set; }
}