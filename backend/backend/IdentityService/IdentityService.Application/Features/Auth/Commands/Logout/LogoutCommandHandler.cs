using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly JwtHelper _jwtHelper;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenBlacklist _tokenBlacklist;

    public LogoutCommandHandler(
        JwtHelper jwtHelper,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenBlacklist tokenBlacklist)
    {
        _jwtHelper = jwtHelper;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenBlacklist = tokenBlacklist;
    }
    /// <summary>
    /// Handles a request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// Response from the request
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Invalid refresh token
    /// or
    /// Invalid token format
    /// or
    /// Token is not a refresh token
    /// or
    /// Refresh token not found
    /// or
    /// Token does not belong to user
    /// </exception>
    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Validate JWT refresh token
        var principal = _jwtHelper.ValidateToken(request.RefreshToken);
        if (principal == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? principal.FindFirst("sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            throw new UnauthorizedAccessException("Invalid token format");

        // Check token type : refresh
        var tokenType = principal.FindFirst("type")?.Value;
        if (tokenType != "refresh")
            throw new UnauthorizedAccessException("Token is not a refresh token");

        // Get token from database
        var token = await _refreshTokenRepository.GetByTokenIncludeRevokedAsync(request.RefreshToken);
        if (token == null)
            throw new UnauthorizedAccessException("Refresh token not found");

        if (token.UserId != userId)
            throw new UnauthorizedAccessException("Token does not belong to user");

        // Already revoked
        if (token.IsRevoked == true)
        {
            if (!string.IsNullOrEmpty(request.AccessToken))
                await BlacklistAccessTokenIfValid(request.AccessToken);

            return false;
        }

        // Expired
        if (token.ExpiresAt < DateTimeOffset.UtcNow)
        {
            token.IsRevoked = true;
            token.UpdatedAt = DateTimeOffset.UtcNow;
            await _refreshTokenRepository.SaveChangesAsync();

            if (!string.IsNullOrEmpty(request.AccessToken))
                await BlacklistAccessTokenIfValid(request.AccessToken);

            return false;
        }

        // Revoke refresh token
        token.IsRevoked = true;
        token.UpdatedAt = DateTimeOffset.UtcNow;
        await _refreshTokenRepository.SaveChangesAsync();

        // Blacklist access token if provided
        if (!string.IsNullOrEmpty(request.AccessToken))
            await BlacklistAccessTokenIfValid(request.AccessToken);

        return true;
    }

    private async Task BlacklistAccessTokenIfValid(string accessToken)
    {
        try
        {
            var tokenExpiry = _jwtHelper.GetTokenExpiry(accessToken);

            if (tokenExpiry.HasValue && tokenExpiry.Value > DateTime.UtcNow)
            {
                await _tokenBlacklist.AddToBlacklistAsync(
                    accessToken,
                    new DateTimeOffset(tokenExpiry.Value)
                );
            }
        }
        catch
        {
            // ignore invalid token
        }
    }
}
