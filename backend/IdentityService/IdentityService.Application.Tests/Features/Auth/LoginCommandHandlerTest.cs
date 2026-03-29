using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Dtos.Auth.Response;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Auth.Commands.Login;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Validators.Auth;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Auth;

public class LoginCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly JwtHelper _jwtHelper;
    private readonly LoginCommandHandler _handler;
    private readonly LoginCommandValidator _validator;

    public LoginCommandHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _refreshRepoMock = new Mock<IRefreshTokenRepository>();
        _mapperMock = new Mock<IMapper>();
        _validator = new LoginCommandValidator();

        // Sử dụng instance thật cho JwtHelper để test logic tạo token
        _jwtHelper = new JwtHelper(
            "this_is_a_very_long_secret_key_for_testing_purposes",
            "Issuer",
            "Audience");

        _handler = new LoginCommandHandler(
            _userRepoMock.Object,
            _refreshRepoMock.Object,
            _jwtHelper,
            _mapperMock.Object);
    }
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_EmailOrUsername_Is_Empty(string value)
    {
        // Arrange
        var request = new LoginReq { EmailOrUsername = value, Password = "password123" };
        var command = new LoginCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.EmailOrUsername)
              .WithErrorMessage("Email or Username is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_Password_Is_Empty(string value)
    {
        // Arrange
        var request = new LoginReq { EmailOrUsername = "user123", Password = value };
        var command = new LoginCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Password)
              .WithErrorMessage("Password is required");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange: Password chỉ có 4 ký tự (vi phạm MinimumLength(5))
        var request = new LoginReq { EmailOrUsername = "user123", Password = "1234" };
        var command = new LoginCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Password)
              .WithErrorMessage("Password must be at least 5 characters");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var request = new LoginReq { EmailOrUsername = "test@example.com", Password = "securePassword" };
        var command = new LoginCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var request = new LoginReq { EmailOrUsername = "unknown", Password = "any" };
        _userRepoMock.Setup(x => x.GetByEmailOrUserNameAsync(request.EmailOrUsername))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new LoginCommand(request), default))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email/username or password");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenAccountIsBlocked()
    {
        // Arrange
        var request = new LoginReq { EmailOrUsername = "blocked_user", Password = "any" };
        var user = new User { IsBlocked = true };
        _userRepoMock.Setup(x => x.GetByEmailOrUserNameAsync(request.EmailOrUsername))
            .ReturnsAsync(user);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new LoginCommand(request), default))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Account is blocked");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenRoleIsNull()
    {
        // Arrange
        var request = new LoginReq { EmailOrUsername = "no_role_user", Password = "any" };
        var user = new User { IsBlocked = false, Role = null };
        _userRepoMock.Setup(x => x.GetByEmailOrUserNameAsync(request.EmailOrUsername))
            .ReturnsAsync(user);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new LoginCommand(request), default))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Account's role is null");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenPasswordIsWrong()
    {
        // Arrange
        var request = new LoginReq { EmailOrUsername = "user", Password = "wrong_password" };
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correct_password");
        var user = new User { IsBlocked = false, Role = new Role { Slug = "admin" }, Password = hashedPassword };

        _userRepoMock.Setup(x => x.GetByEmailOrUserNameAsync(request.EmailOrUsername))
            .ReturnsAsync(user);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new LoginCommand(request), default))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Wrong password");
    }

    [Fact]
    public async Task Handle_ShouldReturnLoginRep_WhenCredentialsAreValid()
    {
        // Arrange
        var password = "valid_password";
        var request = new LoginReq { EmailOrUsername = "valid_user", Password = password };
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            IsBlocked = false,
            Role = new Role { Slug = "staff" },
            Password = hashedPassword,
            HomeStoreId = Guid.NewGuid()
        };

        _userRepoMock.Setup(x => x.GetByEmailOrUserNameAsync(request.EmailOrUsername))
            .ReturnsAsync(user);

        _mapperMock.Setup(m => m.Map<UserRep>(user))
            .Returns(new UserRep { Id = user.Id });

        // Act
        var result = await _handler.Handle(new LoginCommand(request), default);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Id.Should().Be(user.Id);

        _refreshRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }
}