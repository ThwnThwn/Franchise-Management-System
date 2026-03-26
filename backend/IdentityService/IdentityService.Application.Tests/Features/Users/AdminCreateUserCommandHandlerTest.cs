using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Users.Commands.AdminCreateUser;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Validators.Users;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Users;

public class AdminCreateUserCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AdminCreateUserCommandHandler _handler;
    private readonly AdminCreateUserCommandValidator _validator;

    public AdminCreateUserCommandHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new AdminCreateUserCommandHandler(_userRepoMock.Object, _mapperMock.Object, _roleRepoMock.Object);
        _validator = new AdminCreateUserCommandValidator();
    }

    [Theory]
    [InlineData("")]         // Rỗng
    [InlineData(null)]       // Null
    [InlineData("invalid")]  // Sai định dạng email
    public void Should_Have_Error_When_Email_Is_Invalid(string email)
    {
        var model = new AdminCreateUserReq { Email = email };
        var command = new AdminCreateUserCommand(model);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Email);
    }

    [Fact]
    public void Should_Have_Error_When_RoleSlug_Is_Empty()
    {
        var model = new AdminCreateUserReq { Email = "test@test.com", RoleSlug = "" };
        var command = new AdminCreateUserCommand(model);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.RoleSlug)
              .WithErrorMessage("RoleSlug is required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var model = new AdminCreateUserReq
        {
            Email = "admin@company.com",
            RoleSlug = "admin-role"
        };
        var command = new AdminCreateUserCommand(model);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenEmailExists()
    {
        // Arrange
        var req = new AdminCreateUserReq { Email = "duplicate@test.com" };
        _userRepoMock.Setup(x => x.GetByEmailAsync(req.Email)).ReturnsAsync(new User());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new AdminCreateUserCommand(req), default))
            .Should().ThrowAsync<ConflictDuplicateException>()
            .WithMessage(Message.UserMessage.ExistingUser);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenPhoneExists()
    {
        // Arrange
        var req = new AdminCreateUserReq { Email = "new@test.com", Phone = "0123456789", Username = "testuser" };
        _userRepoMock.Setup(x => x.GetByEmailAsync(req.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByPhoneAsync(req.Phone)).ReturnsAsync(new User());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new AdminCreateUserCommand(req), default))
            .Should().ThrowAsync<ConflictDuplicateException>()
            .WithMessage($"Username '{req.Username}' already exists");
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenUsernameExists()
    {
        // Arrange
        var req = new AdminCreateUserReq { Email = "new@test.com", Username = "existing" };
        _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByPhoneAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByUsernameAsync(req.Username)).ReturnsAsync(new User());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new AdminCreateUserCommand(req), default))
            .Should().ThrowAsync<ConflictDuplicateException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRoleNotFound()
    {
        // Arrange
        var req = new AdminCreateUserReq
        {
            Email = "admin@company.com",
            RoleSlug = "admin-role"
        };
        _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByPhoneAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _roleRepoMock.Setup(x => x.GetBySlugAsync(req.RoleSlug)).ReturnsAsync((Role?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new AdminCreateUserCommand(req), default))
            .Should().ThrowAsync<Exception>()
            .WithMessage($"*Slug{req.RoleSlug} not found*");
    }

    [Fact]
    public async Task Handle_ShouldCreateUserSuccessfully_WithFullInformation()
    {
        // Arrange
        var req = new AdminCreateUserReq
        {
            Email = "admin@test.com",
            RoleSlug = "admin",
            CurrentHourlyWage = 15.5m,
            HomeStoreId = Guid.NewGuid()
        };
        var userEntity = new User();
        var role = new Role { Id = Guid.NewGuid(), Slug = "admin" };
        var userRep = new UserRep { Email = req.Email };

        _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByPhoneAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _roleRepoMock.Setup(x => x.GetBySlugAsync(req.RoleSlug)).ReturnsAsync(role);

        _mapperMock.Setup(m => m.Map<User>(req)).Returns(userEntity);
        _mapperMock.Setup(m => m.Map<UserRep>(userEntity)).Returns(userRep);

        // Act
        var result = await _handler.Handle(new AdminCreateUserCommand(req), default);

        // Assert
        result.Should().NotBeNull();
        userEntity.CurrentHourlyWage.Should().Be(req.CurrentHourlyWage);
        userEntity.HomeStoreId.Should().Be(req.HomeStoreId);
        userEntity.RoleId.Should().Be(role.Id);
        userEntity.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));

        _userRepoMock.Verify(x => x.AddWithRoleAsync(userEntity), Times.Once);
    }
}