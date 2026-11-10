using IdentityService.Application.Dtos.Auth.Response;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.TokenRefresh;

public class TokenRefreshCommandHandler : IRequestHandler<TokenRefreshCommand, RefreshTokenRep>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly JwtHelper _jwtHelper;

    public TokenRefreshCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        JwtHelper jwtHelper)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
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
    /// Refresh token has been revoked. All tokens have been invalidated for security.
    /// or
    /// Token does not belong to user
    /// or
    /// Refresh token has expired
    /// or
    /// User not found
    /// or
    /// Account is blocked
    /// or
    /// Account's role is null
    /// </exception>
    public async Task<RefreshTokenRep> Handle(TokenRefreshCommand request, CancellationToken cancellationToken)
    {
        //Validate refresh token format + signature
        var principal = _jwtHelper.ValidateToken(request.RefreshToken);
        if (principal == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        //Extract userId
        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? principal.FindFirst("sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            throw new UnauthorizedAccessException("Invalid token format");

        //Must be refresh token
        var tokenType = principal.FindFirst("type")?.Value;
        if (tokenType != "refresh")
            throw new UnauthorizedAccessException("Token is not a refresh token");

        //Find refresh token in DB
        var token = await _refreshTokenRepository.GetByTokenIncludeRevokedAsync(request.RefreshToken);
        if (token == null)
            throw new UnauthorizedAccessException("Refresh token not found");

        //If token revoked => possible reuse attack => revoke all user tokens
        if (token.IsRevoked == true)
        {
            await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);
            throw new UnauthorizedAccessException(
                "Refresh token has been revoked. All tokens have been invalidated for security.");
        }

        //Token belongs to correct user
        if (token.UserId != userId)
            throw new UnauthorizedAccessException("Token does not belong to user");

        //Expired token
        if (token.ExpiresAt < DateTimeOffset.UtcNow)
        {
            token.IsRevoked = true;
            token.UpdatedAt = DateTimeOffset.UtcNow;
            await _refreshTokenRepository.SaveChangesAsync();

            throw new UnauthorizedAccessException("Refresh token has expired");
        }

        //User still valid
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        if (user.IsBlocked == true)
            throw new UnauthorizedAccessException("Account is blocked");

        if (user.Role == null)
            throw new UnauthorizedAccessException("Account's role is null");

        //Rotation: revoke old refresh token
        token.IsRevoked = true;
        token.UpdatedAt = DateTimeOffset.UtcNow;
        await _refreshTokenRepository.SaveChangesAsync();

        // Gen new token
        var newAccessToken = _jwtHelper.GenerateAccessToken(userId, user.Role.Slug, user.HomeStoreId);
        var (newRefreshTokenValue, newRefreshTokenExpiry) = _jwtHelper.GenerateRefreshToken(userId);

        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenValue,
            UserId = userId,
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = new DateTimeOffset(newRefreshTokenExpiry, TimeSpan.Zero),
            IsRevoked = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken);

        // 11) Return new tokens
        return new RefreshTokenRep
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue
        };
    }
}