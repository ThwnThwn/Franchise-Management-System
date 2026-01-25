namespace IdentityService.Application.Dtos.Auth.Response;

public class RefreshTokenRep
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}

