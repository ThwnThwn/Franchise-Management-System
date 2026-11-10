using FluentAssertions;
using FluentValidation.TestHelper;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Features.Auth.Commands.Logout;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Validators.Auth;
using IdentityService.Domain.Entities;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Xunit;

namespace IdentityService.Application.Tests.Features.Auth;

public class LogoutCommandHandlerTest
{
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock;
    private readonly Mock<ITokenBlacklist> _blacklistMock;
    private readonly JwtHelper _jwtHelper;
    private readonly LogoutCommandHandler _handler;
    private readonly LogoutCommandValidator _validator;

    public LogoutCommandHandlerTest()
    {
        _refreshRepoMock = new Mock<IRefreshTokenRepository>();
        _blacklistMock = new Mock<ITokenBlacklist>();
        _validator = new LogoutCommandValidator();
        // Sử dụng instance thật cho JwtHelper
        _jwtHelper = new JwtHelper("super_secret_key_at_least_32_characters", "Issuer", "Audience");

        _handler = new LogoutCommandHandler(_jwtHelper, _refreshRepoMock.Object, _blacklistMock.Object);
    }
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")] // Kiểm tra cả khoảng trắng
    public void Should_Have_Error_When_RefreshToken_Is_Empty(string refreshToken)
    {
        // Arrange
        var command = new LogoutCommand(refreshToken);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
              .WithErrorMessage("RefreshToken is required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_RefreshToken_Is_Provided()
    {
        // Arrange
        var command = new LogoutCommand("valid_refresh_token_string");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenTokenIsInvalid()
    {
        // Arrange: Truyền một chuỗi không phải JWT
        var command = new LogoutCommand("invalid_token_string");

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid refresh token");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenTokenTypeIsNotRefresh()
    {
        // Arrange: Tạo Access Token thay vì Refresh Token
        var userId = Guid.NewGuid();
        var accessToken = _jwtHelper.GenerateAccessToken(userId, "admin", Guid.NewGuid());
        var command = new LogoutCommand(accessToken);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Token is not a refresh token");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenTokenNotFoundInDb()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (refreshToken, _) = _jwtHelper.GenerateRefreshToken(userId);
        _refreshRepoMock.Setup(x => x.GetByTokenIncludeRevokedAsync(refreshToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new LogoutCommand(refreshToken), default))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenTokenIsAlreadyRevoked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (refreshToken, _) = _jwtHelper.GenerateRefreshToken(userId);
        var dbToken = new RefreshToken { UserId = userId, IsRevoked = true };

        _refreshRepoMock.Setup(x => x.GetByTokenIncludeRevokedAsync(refreshToken))
            .ReturnsAsync(dbToken);

        // Act
        var result = await _handler.Handle(new LogoutCommand(refreshToken, "some_access_token"), default);

        // Assert
        result.Should().BeFalse();
        _refreshRepoMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_AndRevoke_WhenTokenIsExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (refreshToken, _) = _jwtHelper.GenerateRefreshToken(userId);
        var dbToken = new RefreshToken
        {
            UserId = userId,
            IsRevoked = false,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-10) // Đã hết hạn
        };

        _refreshRepoMock.Setup(x => x.GetByTokenIncludeRevokedAsync(refreshToken))
            .ReturnsAsync(dbToken);

        // Act
        var result = await _handler.Handle(new LogoutCommand(refreshToken), default);

        // Assert
        result.Should().BeFalse();
        dbToken.IsRevoked.Should().BeTrue();
        _refreshRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_AndBlacklistAccess_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (refreshToken, _) = _jwtHelper.GenerateRefreshToken(userId);
        var accessToken = _jwtHelper.GenerateAccessToken(userId, "user", Guid.NewGuid());

        var dbToken = new RefreshToken
        {
            UserId = userId,
            IsRevoked = false,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        _refreshRepoMock.Setup(x => x.GetByTokenIncludeRevokedAsync(refreshToken))
            .ReturnsAsync(dbToken);

        // Act
        var result = await _handler.Handle(new LogoutCommand(refreshToken, accessToken), default);

        // Assert
        result.Should().BeTrue();
        dbToken.IsRevoked.Should().BeTrue();

        // Kiểm tra blacklist với DateTimeOffset để tránh lỗi CS8620
        _blacklistMock.Verify(x => x.AddToBlacklistAsync(
            accessToken,
            It.IsAny<DateTimeOffset>()),
            Times.Once);

        _refreshRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void LogoutReq_Should_Pass_Validation_With_Valid_Data()
    {
        // Arrange
        var request = new LogoutReq
        {
            RefreshToken = "valid_refresh_token",
            AccessToken = "optional_access_token"
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void LogoutReq_Should_Fail_Validation_When_RefreshToken_Is_Missing(string invalidToken)
    {
        // Arrange
        var request = new LogoutReq
        {
            RefreshToken = invalidToken! // Thử nghiệm giá trị null hoặc rỗng
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().NotBeEmpty();
        results.Any(v => v.MemberNames.Contains("RefreshToken")).Should().BeTrue();
    }

    [Fact]
    public void LogoutReq_Should_Pass_Validation_When_AccessToken_Is_Null()
    {
        // Arrange: AccessToken là optional nên null vẫn phải pass
        var request = new LogoutReq
        {
            RefreshToken = "required_token",
            AccessToken = null
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }
}