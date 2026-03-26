using FluentAssertions;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Features.Auth.Commands.NewPassword;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Auth;

public class NewPasswordCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock;
    private readonly NewPasswordCommandHandler _handler;

    public NewPasswordCommandHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _refreshRepoMock = new Mock<IRefreshTokenRepository>();
        _handler = new NewPasswordCommandHandler(_userRepoMock.Object, _refreshRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var email = "notfound@example.com";
        var command = new NewPasswordCommand(email, new NewPasswordReq { NewPassword = "Password123" });

        _userRepoMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage($"User with email '{email}' not found");

        _refreshRepoMock.Verify(x => x.RevokeAllUserTokensAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenEmailIsValid()
    {
        // Arrange
        var email = "user@example.com";
        var newPass = "NewSecurePass123";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = email, Password = "OldHashedPassword" };

        var command = new NewPasswordCommand(email, new NewPasswordReq { NewPassword = newPass });

        _userRepoMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().Be(Unit.Value);

        // 1. Kiểm tra đã Revoke toàn bộ token cũ
        _refreshRepoMock.Verify(x => x.RevokeAllUserTokensAsync(userId), Times.Once);

        // 2. Kiểm tra mật khẩu mới đã được băm (BCrypt) và gán vào user
        BCrypt.Net.BCrypt.Verify(newPass, user.Password).Should().BeTrue();

        // 3. Kiểm tra đã gọi Update lưu vào DB
        _userRepoMock.Verify(x => x.UpdateAsync(It.Is<User>(u =>
            u.Email == email &&
            u.Id == userId)), Times.Once);
    }
}