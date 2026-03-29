using FluentAssertions;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Features.Auth.Commands.ReSendOtp;
using IdentityService.Application.Persistences.Caches;
using MediatR;
using Moq;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using Xunit;

namespace IdentityService.Application.Tests.Features.Auth;

public class ReSendOtpCommandHandlerTest
{
    private readonly Mock<IOtpStore> _otpStoreMock;
    private readonly Mock<IKafkaProducer> _kafkaProducerMock;
    private readonly ReSendOtpCommandHandler _handler;

    public ReSendOtpCommandHandlerTest()
    {
        _otpStoreMock = new Mock<IOtpStore>();
        _kafkaProducerMock = new Mock<IKafkaProducer>();

        // Mock IEmailService vì nó được inject nhưng chưa dùng tới trong code hiện tại
        var emailServiceMock = new Mock<IEmailService>();

        _handler = new ReSendOtpCommandHandler(
            _otpStoreMock.Object,
            emailServiceMock.Object,
            _kafkaProducerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenResendingTooSoon()
    {
        // Arrange
        var email = "test@example.com";
        var purpose = "register";
        var request = new ReSendOtpReq { Email = email, Purpose = purpose };

        // Giả lập vừa gửi OTP cách đây 10 giây (chưa đủ 30 giây)
        var lastOtpTime = DateTimeOffset.UtcNow.AddSeconds(-10);
        _otpStoreMock.Setup(x => x.GetLastOtpRequestTimeAsync(email, purpose))
            .ReturnsAsync(lastOtpTime);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new ReSendOtpCommand(request), default))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("Please wait 30 seconds before resending OTP.");

        _kafkaProducerMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<OtpRequestedEvent>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenOtpNotFound()
    {
        // Arrange
        var email = "test@example.com";
        var purpose = "forget-password";
        var request = new ReSendOtpReq { Email = email, Purpose = purpose };

        // Đã qua thời gian chờ nhưng OTP trong cache đã bị xóa hoặc hết hạn
        _otpStoreMock.Setup(x => x.GetLastOtpRequestTimeAsync(email, purpose))
            .ReturnsAsync(DateTimeOffset.UtcNow.AddSeconds(-40));

        _otpStoreMock.Setup(x => x.GetOtpAsync(email, purpose))
            .ReturnsAsync((string?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new ReSendOtpCommand(request), default))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("Otp data not found or expired");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenRequestIsValid()
    {
        // Arrange
        var email = "valid@test.com";
        var purpose = "register";
        var request = new ReSendOtpReq { Email = email, Purpose = purpose };

        _otpStoreMock.Setup(x => x.GetLastOtpRequestTimeAsync(email, purpose))
            .ReturnsAsync((DateTimeOffset?)null); // Hoặc một mốc thời gian rất xa

        _otpStoreMock.Setup(x => x.GetOtpAsync(email, purpose))
            .ReturnsAsync("OLD_OTP");

        // Act
        var result = await _handler.Handle(new ReSendOtpCommand(request), default);

        // Assert
        result.Should().Be(Unit.Value);

        // Kiểm tra trình tự: Xóa OTP cũ -> Lưu OTP mới -> Reset attempt -> Publish Kafka
        _otpStoreMock.Verify(x => x.RemoveOTPAsync(email, purpose), Times.Once);
        _otpStoreMock.Verify(x => x.SaveOTPAsyns(email, It.IsAny<string>(), purpose), Times.Once);
        _otpStoreMock.Verify(x => x.ResetOtpAttemptAsync(email, purpose), Times.Once);

        _kafkaProducerMock.Verify(x => x.PublishAsync(
            KafkaTopics.OtpNotifications,
            It.IsAny<OtpRequestedEvent>()), Times.Once);
    }
}