using FluentAssertions;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Features.Auth.Commands.ForgetPassword;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;
using Moq;
using SharedKernel.Infrastructure.Kafka;
using SharedKernel.Events;
using Xunit;

namespace IdentityService.Application.Tests.Features.Auth;

public class ForgetPasswordCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IOtpStore> _otpStoreMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IKafkaProducer> _kafkaProducerMock;
    private readonly ForgetPasswordCommandHandler _handler;

    public ForgetPasswordCommandHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _otpStoreMock = new Mock<IOtpStore>();
        _emailServiceMock = new Mock<IEmailService>();
        _kafkaProducerMock = new Mock<IKafkaProducer>();

        _handler = new ForgetPasswordCommandHandler(
            _userRepoMock.Object,
            _otpStoreMock.Object,
            _emailServiceMock.Object,
            _kafkaProducerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenRequestingTooFast()
    {
        // Arrange
        var email = "test@example.com";
        var request = new ForgetPasswordReq { Email = email };
        var command = new ForgetPasswordCommand(request);

        // Giả lập yêu cầu OTP cách đây 2 phút (chưa đủ 5 phút)
        var lastRequestTime = DateTimeOffset.UtcNow.AddMinutes(-2);
        _otpStoreMock.Setup(x => x.GetLastOtpRequestTimeAsync(email, "forget-password"))
            .ReturnsAsync(lastRequestTime);

        // Act & Assert
        var action = () => _handler.Handle(command, default);
        await action.Should().ThrowAsync<BusinessException>()
            .WithMessage("Please wait 3 minute(s) before requesting a new OTP.");
    }

    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenEmailDoesNotExist()
    {
        // Arrange
        var email = "notfound@example.com";
        var command = new ForgetPasswordCommand(new ForgetPasswordReq { Email = email });

        _otpStoreMock.Setup(x => x.GetLastOtpRequestTimeAsync(email, It.IsAny<string>()))
            .ReturnsAsync((DateTimeOffset?)null);

        _userRepoMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage($"Email '{email}' does not exist");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenEmailIsValid()
    {
        // Arrange
        var email = "user@example.com";
        var command = new ForgetPasswordCommand(new ForgetPasswordReq { Email = email });
        var user = new User { Email = email };

        _otpStoreMock.Setup(x => x.GetLastOtpRequestTimeAsync(email, It.IsAny<string>()))
            .ReturnsAsync((DateTimeOffset?)null);

        _userRepoMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().Be(Unit.Value);

        // Verify các tương tác với Cache/OTP Store
        _otpStoreMock.Verify(x => x.SaveOTPAsyns(email, It.IsAny<string>(), "forget-password"), Times.Once);
        _otpStoreMock.Verify(x => x.SaveLastOtpRequestTimeAsync(email, It.IsAny<DateTimeOffset>(), "forget-password"), Times.Once);

        // Verify Kafka đã được gọi
        _kafkaProducerMock.Verify(x => x.PublishAsync(
            It.IsAny<string>(),
            It.IsAny<OtpRequestedEvent>()), Times.Once);
    }
}