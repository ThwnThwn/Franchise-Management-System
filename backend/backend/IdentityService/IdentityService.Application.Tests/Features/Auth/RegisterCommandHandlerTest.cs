using FluentAssertions;
using FluentValidation.TestHelper;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Features.Auth.Commands.Register;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Validators.Auth;
using IdentityService.Domain.Entities;
using Moq;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using Xunit;

namespace IdentityService.Application.Tests.Features.Auth;

public class RegisterCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IOtpStore> _otpStoreMock;
    private readonly Mock<IKafkaProducer> _kafkaProducerMock;
    private readonly RegisterCommandHandler _handler;
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        _otpStoreMock = new Mock<IOtpStore>();
        _kafkaProducerMock = new Mock<IKafkaProducer>();
        _validator = new RegisterCommandValidator();
        _handler = new RegisterCommandHandler(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _otpStoreMock.Object,
            new Mock<IdentityService.Application.ExternalServices.IEmailService>().Object,
            _kafkaProducerMock.Object);
    }

    private RegisterReq CreateValidRequest() => new()
    {
        Email = "new@test.com",
        Username = "newuser",
        Phone = "0123456789",
        Password = "password123",
        FirstName = "John",
        Address = "123 Street",
        Gender = 1,
        DateOrBirth = DateTime.UtcNow.AddYears(-20)
    };

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid-email")]
    public void Should_Have_Error_When_Email_Is_Invalid(string email)
    {
        // Arrange
        var request = new RegisterReq { Email = email };
        var command = new RegisterCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_Password_Is_Empty(string password)
    {
        // Arrange
        var request = new RegisterReq { Password = password };
        var command = new RegisterCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Password)
              .WithErrorMessage("Password is required");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange
        var request = new RegisterReq { Password = "123" }; // < 5 ký tự
        var command = new RegisterCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Password)
              .WithErrorMessage("Password must be at least 5 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_Phone_Is_Empty(string phone)
    {
        // Arrange
        var request = new RegisterReq { Phone = phone };
        var command = new RegisterCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Phone)
              .WithErrorMessage("Phone is required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var request = new RegisterReq
        {
            Email = "test@example.com",
            Password = "securePassword",
            Phone = "0123456789"
        };
        var command = new RegisterCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenOtpRequestedTooSoon()
    {
        // Arrange
        var request = CreateValidRequest();
        var lastTime = DateTimeOffset.UtcNow.AddMinutes(-2); // Mới gửi 2 phút trước
        _otpStoreMock.Setup(x => x.GetLastOtpRequestTimeAsync(request.Email, "register"))
            .ReturnsAsync(lastTime);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new RegisterCommand(request), default))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("*Please wait 3 minute(s)*");
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenEmailExists()
    {
        // Arrange
        var request = CreateValidRequest();
        _otpStoreMock.Setup(x => x.GetLastOtpRequestTimeAsync(It.IsAny<string>(), "register"))
            .ReturnsAsync((DateTimeOffset?)null);
        _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(new User());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new RegisterCommand(request), default))
            .Should().ThrowAsync<ConflictDuplicateException>()
            .WithMessage($"Email '{request.Email}' already exists");
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenUsernameExists()
    {
        // Arrange
        var request = CreateValidRequest();
        _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByUsernameAsync(request.Username)).ReturnsAsync(new User());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new RegisterCommand(request), default))
            .Should().ThrowAsync<ConflictDuplicateException>()
            .WithMessage($"Username '{request.Username}' already exists");
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenPhoneExists()
    {
        // Arrange
        var request = CreateValidRequest();
        _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByPhoneAsync(request.Phone)).ReturnsAsync(new User());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new RegisterCommand(request), default))
            .Should().ThrowAsync<ConflictDuplicateException>()
            .WithMessage($"Username with phone'{request.Phone}' already exists");
    }

    [Fact]
    public async Task Handle_ShouldThrowForbidden_WhenRoleNotFound()
    {
        // Arrange
        var request = CreateValidRequest();
        _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByPhoneAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _roleRepoMock.Setup(x => x.GetByNameAsync("Customer")).ReturnsAsync((Role?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new RegisterCommand(request), default))
            .Should().ThrowAsync<ForbidenException>();
    }

    [Fact]
    public async Task Handle_ShouldSucceed_AndSaveTempData_AndPublishKafka()
    {
        // Arrange
        var request = CreateValidRequest();
        _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByPhoneAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _roleRepoMock.Setup(x => x.GetByNameAsync("Customer")).ReturnsAsync(new Role { Id = Guid.NewGuid() });

        // Act
        var result = await _handler.Handle(new RegisterCommand(request), default);

        // Assert
        result.Should().NotBeNull();

        // Kiểm tra lưu dữ liệu tạm vào Redis
        _otpStoreMock.Verify(x => x.SaveRegistrationDataAsync(request.Email, It.IsAny<string>()), Times.Once);
        _otpStoreMock.Verify(x => x.SaveOTPAsyns(request.Email, It.IsAny<string>(), "register"), Times.Once);

        // Kiểm tra gửi event qua Kafka
        _kafkaProducerMock.Verify(x => x.PublishAsync(
            It.IsAny<string>(),
            It.IsAny<OtpRequestedEvent>()), Times.Once);
    }
}