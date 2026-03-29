using AutoMapper;
using FluentAssertions;
using IdentityService.Application.Abstrations;
using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Features.Users.Commands.ChangePassword;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using Moq;

namespace IdentityService.Application.Tests.Features.Users;

public class ChangePasswordCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock;
    private readonly Mock<ITokenBlacklist> _blacklistMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly JwtHelper _jwtHelper; // Thường dùng instance thật nếu không phụ thuộc IO phức tạp
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        _refreshRepoMock = new Mock<IRefreshTokenRepository>();
        _blacklistMock = new Mock<ITokenBlacklist>();
        _mapperMock = new Mock<IMapper>();

        // Giả sử JwtHelper có thể khởi tạo đơn giản, nếu không hãy Mock nó
        _jwtHelper = new JwtHelper(
            "super_secret_key_at_least_32_characters_long", // secretKey
            "IdentityService",                               // issuer
            "IdentityClient"                                 // audience
        );

        _handler = new ChangePasswordCommandHandler(
            _userRepoMock.Object,
            _mapperMock.Object,
            new Mock<ICurrentUserService>().Object,
            _roleRepoMock.Object,
            _refreshRepoMock.Object,
            _blacklistMock.Object,
            _jwtHelper);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        var command = new ChangePasswordComand(Guid.NewGuid(), new ChangePasswordReq());
        _userRepoMock.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync((User?)null);

        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Unauthenticated");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenOldPasswordIsWrong()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Password = BCrypt.Net.BCrypt.HashPassword("correct_pass") };
        var req = new ChangePasswordReq { OldPassword = "wrong_pass" };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        await _handler.Invoking(x => x.Handle(new ChangePasswordComand(userId, req), default))
            .Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid wrong current password");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenPasswordsDoNotMatch()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Password = BCrypt.Net.BCrypt.HashPassword("old_pass") };
        var req = new ChangePasswordReq { OldPassword = "old_pass", NewPassword = "new1", ConfirmNewPassword = "new2" };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        await _handler.Invoking(x => x.Handle(new ChangePasswordComand(userId, req), default))
            .Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("New password and Confirm password are not the same");
    }

    [Fact]
    public async Task Handle_ShouldChangePasswordSuccessfully_AndRevokeTokens_WhenForgetAllIsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldPass = "old_pass";
        var newPass = "new_pass_123";
        var user = new User { Id = userId, Password = BCrypt.Net.BCrypt.HashPassword(oldPass) };
        var req = new ChangePasswordReq
        {
            OldPassword = oldPass,
            NewPassword = newPass,
            ConfirmNewPassword = newPass,
            ForgetAll = true,
            AccessToken = "valid_token"
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Giả lập JwtHelper trả về expiry (Cần cẩn thận nếu JwtHelper logic phức tạp)
        // Nếu JwtHelper là class thực, bạn cần cung cấp token hợp lệ hoặc Mock JwtHelper nếu nó là Virtual

        // Act
        await _handler.Handle(new ChangePasswordComand(userId, req), default);

        // Assert
        BCrypt.Net.BCrypt.Verify(newPass, user.Password).Should().BeTrue();
        _userRepoMock.Verify(x => x.UpdateAsync(user), Times.Once);
        _refreshRepoMock.Verify(x => x.RevokeAllUserTokensAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotRevokeTokens_WhenForgetAllIsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        // Cần Password băm đúng chuẩn để vượt qua bước kiểm tra Verify bên trong Handler
        var user = new User { Id = userId, Password = BCrypt.Net.BCrypt.HashPassword("old_pass") };
        var req = new ChangePasswordReq
        {
            OldPassword = "old_pass",
            NewPassword = "new_pass",
            ConfirmNewPassword = "new_pass",
            ForgetAll = false
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        await _handler.Handle(new ChangePasswordComand(userId, req), default);

        // Assert
        // Đảm bảo mật khẩu đã được cập nhật thành công
        BCrypt.Net.BCrypt.Verify("new_pass", user.Password).Should().BeTrue();

        // FIX LỖI CS1929 / ArgumentException TẠI ĐÂY:
        // Sử dụng DateTimeOffset thay vì DateTime để khớp với Interface
        _refreshRepoMock.Verify(x => x.RevokeAllUserTokensAsync(It.IsAny<Guid>()), Times.Never);
        _blacklistMock.Verify(x => x.AddToBlacklistAsync(
            It.IsAny<string>(),
            It.IsAny<DateTimeOffset>()), // Đã sửa từ DateTime -> DateTimeOffset
            Times.Never);
    }
}