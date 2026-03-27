using FluentAssertions;
using IdentityService.Application.Dtos.Auth.Response;
using IdentityService.Application.Features.Auth.Commands.TokenRefresh;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using Moq;
using System.Security.Claims;
using Xunit;

namespace IdentityService.Application.Tests.Features.Auth;

public class TokenRefreshCommandHandlerTest
{
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly JwtHelper _jwtHelper;
    private readonly TokenRefreshCommandHandler _handler;

    public TokenRefreshCommandHandlerTest()
    {
        _refreshRepoMock = new Mock<IRefreshTokenRepository>();
        _userRepoMock = new Mock<IUserRepository>();

        // Sử dụng JwtHelper thật để test tính đúng đắn của Claims
        _jwtHelper = new JwtHelper("super_secret_key_at_least_32_characters", "Issuer", "Audience");

        _handler = new TokenRefreshCommandHandler(
            _refreshRepoMock.Object,
            _userRepoMock.Object,
            _jwtHelper);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenTokenIsRevoked_ReuseAttack()
    {
        // Arrange: Giả lập một token đã bị thu hồi (IsRevoked = true)
        var userId = Guid.NewGuid();
        var (refreshToken,_ ) = _jwtHelper.GenerateRefreshToken(userId);
        var dbToken = new RefreshToken { UserId = userId, IsRevoked = true };

        _refreshRepoMock.Setup(x => x.GetByTokenIncludeRevokedAsync(refreshToken))
            .ReturnsAsync(dbToken);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new TokenRefreshCommand(refreshToken), default))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token has been revoked. All tokens have been invalidated for security.");

        // Quan trọng: Kiểm tra xem có gọi RevokeAllUserTokens không (cơ chế bảo mật)
        _refreshRepoMock.Verify(x => x.RevokeAllUserTokensAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenTokenExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (refreshToken, _) = _jwtHelper.GenerateRefreshToken(userId);
        var dbToken = new RefreshToken
        {
            UserId = userId,
            IsRevoked = false,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5) // Đã hết hạn
        };

        _refreshRepoMock.Setup(x => x.GetByTokenIncludeRevokedAsync(refreshToken))
            .ReturnsAsync(dbToken);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new TokenRefreshCommand(refreshToken), default))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token has expired");

        dbToken.IsRevoked.Should().BeTrue(); // Kiểm tra xem code có đánh dấu revoked không
        _refreshRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsBlocked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (refreshToken, _) = _jwtHelper.GenerateRefreshToken(userId);
        var dbToken = new RefreshToken { UserId = userId, IsRevoked = false, ExpiresAt = DateTimeOffset.UtcNow.AddDays(1) };
        var user = new User { Id = userId, IsBlocked = true };

        _refreshRepoMock.Setup(x => x.GetByTokenIncludeRevokedAsync(refreshToken)).ReturnsAsync(dbToken);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new TokenRefreshCommand(refreshToken), default))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Account is blocked");
    }

    [Fact]
    public async Task Handle_ShouldReturnNewTokens_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (oldRefreshTokenValue,_) = _jwtHelper.GenerateRefreshToken(userId);

        var dbToken = new RefreshToken
        {
            UserId = userId,
            IsRevoked = false,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
        };

        var user = new User
        {
            Id = userId,
            IsBlocked = false,
            Role = new Role { Slug = "Customer" },
            HomeStoreId = Guid.NewGuid()
        };

        _refreshRepoMock.Setup(x => x.GetByTokenIncludeRevokedAsync(oldRefreshTokenValue)).ReturnsAsync(dbToken);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(new TokenRefreshCommand(oldRefreshTokenValue), default);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(oldRefreshTokenValue); // Phải là token mới (Rotation)

        // Kiểm tra token cũ bị thu hồi và token mới được add
        dbToken.IsRevoked.Should().BeTrue();
        _refreshRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        _refreshRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}