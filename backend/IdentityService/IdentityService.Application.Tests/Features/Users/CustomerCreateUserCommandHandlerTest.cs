using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Users.Commands.CustomerCreateUser;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Validators.Users;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Users;

public class CustomerCreateUserCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CustomerCreateUserCommandHandler _handler;
    private readonly CustomerCreateUserCommandValidator _validator;
    public CustomerCreateUserCommandHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        _validator = new CustomerCreateUserCommandValidator();
        _mapperMock = new Mock<IMapper>();
        _handler = new CustomerCreateUserCommandHandler(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _mapperMock.Object);
    }

    [Theory]
    [InlineData("")]         // Trống
    [InlineData("invalid")]  // Sai định dạng
    public void Should_Have_Error_When_Email_Is_Invalid(string email)
    {
        var request = new CustomerCreateUserReq { Email = email };
        var command = new CustomerCreateUserCommand(request);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Email);
    }

    [Theory]
    [InlineData("")]      // Trống (NotEmpty)
    [InlineData("abc")]   // Ngắn hơn 5 (MinimumLength)
    public void Should_Have_Error_When_RoleSlug_Is_Invalid(string roleSlug)
    {
        var request = new CustomerCreateUserReq
        {
            Email = "valid@gmail.com", 
            RoleSlug = roleSlug
        };
        var command = new CustomerCreateUserCommand(request);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.RoleSlug);
    }

    [Fact]
    public void Should_Have_Error_When_Phone_Is_Too_Long()
    {
        // Phủ nhánh .When(!string.IsNullOrWhiteSpace) và .MaximumLength(20)
        var request = new CustomerCreateUserReq { Phone = new string('1', 21) };
        var command = new CustomerCreateUserCommand(request);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Phone)
              .WithErrorMessage("Phone is too long");
    }

    [Fact]
    public void Should_Have_Error_When_Address_Is_Too_Long()
    {
        // Phủ nhánh .When(!string.IsNullOrWhiteSpace) và .MaximumLength(255)
        var request = new CustomerCreateUserReq
        {
            Email = "valid@gmail.com",
            RoleSlug = "customer-slug",
            Address = new string('A', 256)
        };
        var command = new CustomerCreateUserCommand(request);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Address)
              .WithErrorMessage("Address is too long");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Optional_Fields_Are_Null()
    {
        // Phủ nhánh false của các điều kiện .When(...)
        var request = new CustomerCreateUserReq
        {
            Email = "customer@example.com",
            RoleSlug = "customer-role",
            Phone = null,
            Address = "  " // WhiteSpace
        };
        var command = new CustomerCreateUserCommand(request);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Request.Phone);
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Address);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var request = new CustomerCreateUserReq
        {
            Email = "valid@example.com",
            RoleSlug = "customer",
            Phone = "0987654321",
            Address = "123 Main St"
        };
        var command = new CustomerCreateUserCommand(request);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenEmailExists()
    {
        // Arrange
        var req = new CustomerCreateUserReq { Email = "exist@test.com" };
        _userRepoMock.Setup(x => x.GetByEmailAsync(req.Email))
            .ReturnsAsync(new User());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new CustomerCreateUserCommand(req), default))
            .Should().ThrowAsync<ConflictDuplicateException>()
            .WithMessage(Message.UserMessage.ExistingUser);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenUsernameExists()
    {
        // Arrange
        var req = new CustomerCreateUserReq { Email = "new@test.com", Username = "exist_user" };
        _userRepoMock.Setup(x => x.GetByEmailAsync(req.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByUsernameAsync(req.Username)).ReturnsAsync(new User());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new CustomerCreateUserCommand(req), default))
            .Should().ThrowAsync<ConflictDuplicateException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRoleNotFound()
    {
        // Arrange
        var req = new CustomerCreateUserReq { Email = "new@test.com", Username = "new_user", RoleSlug = "invalid" };
        _userRepoMock.Setup(x => x.GetByEmailAsync(req.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByUsernameAsync(req.Username)).ReturnsAsync((User?)null);
        _roleRepoMock.Setup(x => x.GetBySlugAsync(req.RoleSlug)).ReturnsAsync((Role?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new CustomerCreateUserCommand(req), default))
            .Should().ThrowAsync<Exception>()
            .WithMessage("*invalid not found*");
    }

    [Fact]
    public async Task Handle_ShouldCreateUserSuccessfully_WhenDataIsValid()
    {
        // Arrange
        var req = new CustomerCreateUserReq
        {
            Email = "valid@test.com",
            Username = "valid_user",
            RoleSlug = "customer"
        };
        var userEntity = new User { Email = req.Email };
        var role = new Role { Id = Guid.NewGuid(), Slug = "customer" };
        var userRep = new UserRep { Email = req.Email };

        _userRepoMock.Setup(x => x.GetByEmailAsync(req.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByUsernameAsync(req.Username)).ReturnsAsync((User?)null);
        _roleRepoMock.Setup(x => x.GetBySlugAsync(req.RoleSlug)).ReturnsAsync(role);

        _mapperMock.Setup(m => m.Map<User>(req)).Returns(userEntity);
        _mapperMock.Setup(m => m.Map<UserRep>(userEntity)).Returns(userRep);

        // Act
        var result = await _handler.Handle(new CustomerCreateUserCommand(req), default);

        // Assert
        result.Should().NotBeNull();
        userEntity.Password.Should().NotBeNullOrEmpty();
        userEntity.RoleId.Should().Be(role.Id);
        userEntity.IsBlocked.Should().BeFalse();

        // Kiểm tra timestamp với sai số (đã xử lý lỗi DateTime ở lượt trước)
        userEntity.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));

        _userRepoMock.Verify(x => x.AddWithRoleAsync(userEntity), Times.Once);
    }
}