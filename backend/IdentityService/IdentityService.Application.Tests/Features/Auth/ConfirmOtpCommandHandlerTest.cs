using AutoMapper;
using FluentAssertions;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Features.Auth.Commands.ConfirmOtp;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;
using Moq;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using System.Text.Json;
using Xunit;

namespace IdentityService.Application.Tests.Features.Auth
{
    public class ConfirmOtpCommandHandlerTest
    {
        private readonly Mock<IOtpStore> _otpStoreMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IKafkaProducer> _kafkaProducerMock;
        private readonly ConfirmOtpCommandHandler _handler;

        public ConfirmOtpCommandHandlerTest()
        {
            _otpStoreMock = new Mock<IOtpStore>();
            _userRepoMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _kafkaProducerMock = new Mock<IKafkaProducer>();

            _handler = new ConfirmOtpCommandHandler(
                _otpStoreMock.Object,
                _userRepoMock.Object,
                _mapperMock.Object,
                _kafkaProducerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldThrowBusinessException_WhenTooManyAttempts()
        {
            // Arrange
            var req = new ConfirmOtpReq { Email = "test@test.com", Otp = "123456" };
            _otpStoreMock.Setup(x => x.GetOtpAttemptCountAsync(req.Email, "register")).ReturnsAsync(5);

            // Act & Assert
            await _handler.Invoking(x => x.Handle(new ConfirmOtpCommand(req), default))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage("Too many failed attempts. Please request a new OTP.");

            _otpStoreMock.Verify(x => x.RemoveOTPAsync(req.Email, "register"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowBusinessException_WhenOtpIsInvalid()
        {
            // Arrange
            var req = new ConfirmOtpReq { Email = "test@test.com", Otp = "wrong" };
            _otpStoreMock.Setup(x => x.GetOtpAttemptCountAsync(req.Email, "register")).ReturnsAsync(0);
            _otpStoreMock.Setup(x => x.GetOtpAsync(req.Email, "register")).ReturnsAsync("correct");

            // Act & Assert
            await _handler.Invoking(x => x.Handle(new ConfirmOtpCommand(req), default))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage("Invalid or expired OTP. 4 attempt(s) remaining.");

            _otpStoreMock.Verify(x => x.IncrementOtpAttemptAsync(req.Email, "register"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowBusinessException_WhenRegistrationDataNotFound()
        {
            // Arrange
            var req = new ConfirmOtpReq { Email = "test@test.com", Otp = "123456" };
            _otpStoreMock.Setup(x => x.GetOtpAttemptCountAsync(req.Email, "register")).ReturnsAsync(0);
            _otpStoreMock.Setup(x => x.GetOtpAsync(req.Email, "register")).ReturnsAsync("123456");
            _otpStoreMock.Setup(x => x.GetRegistrationDataAsync(req.Email)).ReturnsAsync((string?)null);

            // Act & Assert
            await _handler.Invoking(x => x.Handle(new ConfirmOtpCommand(req), default))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage("Registration data not found or expired");
        }

        [Fact]
        public async Task Handle_ShouldThrowConflict_WhenUserAlreadyExists()
        {
            // Arrange
            var req = new ConfirmOtpReq { Email = "exist@test.com", Otp = "123456" };
            var userData = new User { Email = req.Email };
            var json = JsonSerializer.Serialize(userData);

            _otpStoreMock.Setup(x => x.GetOtpAttemptCountAsync(req.Email, "register")).ReturnsAsync(0);
            _otpStoreMock.Setup(x => x.GetOtpAsync(req.Email, "register")).ReturnsAsync("123456");
            _otpStoreMock.Setup(x => x.GetRegistrationDataAsync(req.Email)).ReturnsAsync(json);
            _userRepoMock.Setup(x => x.GetByEmailAsync(req.Email)).ReturnsAsync(new User());

            // Act & Assert
            await _handler.Invoking(x => x.Handle(new ConfirmOtpCommand(req), default))
                .Should().ThrowAsync<ConflictDuplicateException>();
        }

        [Fact]
        public async Task Handle_ShouldSucceed_AndPublishEvents()
        {
            // Arrange
            var req = new ConfirmOtpReq { Email = "new@test.com", Otp = "123456" };
            var userData = new User { Email = req.Email, Password = "hashed_password", Username = "newuser" };
            var json = JsonSerializer.Serialize(userData);

            _otpStoreMock.Setup(x => x.GetOtpAttemptCountAsync(req.Email, "register")).ReturnsAsync(0);
            _otpStoreMock.Setup(x => x.GetOtpAsync(req.Email, "register")).ReturnsAsync("123456");
            _otpStoreMock.Setup(x => x.GetRegistrationDataAsync(req.Email)).ReturnsAsync(json);
            _userRepoMock.Setup(x => x.GetByEmailAsync(req.Email)).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(new ConfirmOtpCommand(req), default);

            // Assert
            result.Should().Be(Unit.Value);
            _userRepoMock.Verify(x => x.AddWithRoleAsync(It.IsAny<User>()), Times.Once);
            _kafkaProducerMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<UserRegisteredEvent>()), Times.Exactly(2));
            _otpStoreMock.Verify(x => x.ResetOtpAttemptAsync(req.Email, "register"), Times.Once);
        }
    }
}