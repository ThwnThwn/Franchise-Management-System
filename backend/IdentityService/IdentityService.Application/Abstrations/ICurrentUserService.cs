namespace IdentityService.Application.Abstrations;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
