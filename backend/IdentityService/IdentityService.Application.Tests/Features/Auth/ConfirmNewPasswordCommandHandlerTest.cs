using FluentAssertions;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Dtos.Auth.Response;
using IdentityService.Application.Features.Auth.Commands.ConfirmNewPassword;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using Moq;
using System.Text.Json;
using Xunit;

namespace IdentityService.Application.Tests.Features.Auth;

public class ConfirmNewPasswordCommandHandlerTest
{
    private readonly Mock<IOtpStore> _otpStoreMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly JwtHelper _jwtHelper;
    private readonly ConfirmNewPasswordCommandHandler _handler;

    public ConfirmNewPasswordCommandHandlerTest()
    {
        _otpStoreMock = new Mock<IOtpStore>();
        _userRepoMock = new Mock<IUserRepository>();

        // Sử dụng instance thật cho JwtHelper để tạo token thực tế
        _jwtHelper = new JwtHelper("secret_key_must_be_long_enough_32_chars", "Issuer", "Audience");

        _handler = new ConfirmNewPasswordCommandHandler(
            _otpStoreMock.Object,
            _userRepoMock.Object,
            _jwtHelper);
    }

    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenTooManyAttempts()
    {
        // Arrange
        var req = new ConfirmOtpReq { Email = "test@example.com", Otp = "123456" };
        _otpStoreMock.Setup(x => x.GetOtpAttemptCountAsync(req.Email, "forget-password")).ReturnsAsync(5);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new ConfirmNewPasswordCommand(req), default))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("Too many failed attempts. Please request a new OTP.");

        _otpStoreMock.Verify(x => x.RemoveOTPAsync(req.Email, "forget-password"), Times.Once);
        _otpStoreMock.Verify(x => x.RemoveForgetPasswordDataAsync(req.Email), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenOtpIsInvalid()
    {
        // Arrange
        var req = new ConfirmOtpReq { Email = "test@example.com", Otp = "wrong" };
        _otpStoreMock.Setup(x => x.GetOtpAttemptCountAsync(req.Email, "forget-password")).ReturnsAsync(0);
        _otpStoreMock.Setup(x => x.GetOtpAsync(req.Email, "forget-password")).ReturnsAsync("correct");

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new ConfirmNewPasswordCommand(req), default))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("Invalid or expired OTP. 4 attempt(s) remaining.");

        _otpStoreMock.Verify(x => x.IncrementOtpAttemptAsync(req.Email, "forget-password"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenSessionExpired()
    {
        // Arrange
        var req = new ConfirmOtpReq { Email = "test@example.com", Otp = "123456" };
        _otpStoreMock.Setup(x => x.GetOtpAttemptCountAsync(req.Email, "forget-password")).ReturnsAsync(0);
        _otpStoreMock.Setup(x => x.GetOtpAsync(req.Email, "forget-password")).ReturnsAsync("123456");
        _otpStoreMock.Setup(x => x.GetForgetPasswordAsync(req.Email)).ReturnsAsync((string?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new ConfirmNewPasswordCommand(req), default))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("Forget password session expired. Please request OTP again.");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_AndReturnForgetPasswordToken()
    {
        // Arrange
        var email = "user@example.com";
        var req = new ConfirmOtpReq { Email = email, Otp = "123456" };
        var forgetData = new ForgetPasswordReq { Email = email };
        var json = JsonSerializer.Serialize(forgetData);

        _otpStoreMock.Setup(x => x.GetOtpAttemptCountAsync(email, "forget-password")).ReturnsAsync(0);
        _otpStoreMock.Setup(x => x.GetOtpAsync(email, "forget-password")).ReturnsAsync("123456");
        _otpStoreMock.Setup(x => x.GetForgetPasswordAsync(email)).ReturnsAsync(json);

        // Act
        var result = await _handler.Handle(new ConfirmNewPasswordCommand(req), default);

        // Assert
        result.Should().NotBeNull();
        result.AccessForgetPasswordToken.Should().NotBeNullOrEmpty();

        // Verify clean up
        _otpStoreMock.Verify(x => x.RemoveOTPAsync(email, "forget-password"), Times.Once);
        _otpStoreMock.Verify(x => x.ResetOtpAttemptAsync(email, "forget-password"), Times.Once);
        _otpStoreMock.Verify(x => x.RemoveForgetPasswordDataAsync(email), Times.Once);
    }
}