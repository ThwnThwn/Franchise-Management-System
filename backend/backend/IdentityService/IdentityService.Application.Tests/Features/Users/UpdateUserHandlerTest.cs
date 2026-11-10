using Moq;
using Xunit;
using FluentAssertions;
using AutoMapper;
using IdentityService.Application.Features.Users.Commands.UpdateUser;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Abstrations;
using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Domain.Entities;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;

namespace IdentityService.Application.Tests.Features.Users;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _mapperMock = new Mock<IMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new UpdateUserCommandHandler(
            _userRepositoryMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object,
            _roleRepositoryMock.Object);
    }

    // 1. TRƯỜNG HỢP THÀNH CÔNG (Full Branch)
    [Fact]
    public async Task Handle_ShouldUpdateUser_WhenAllValidationsPass()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserReq { Email = "new@test.com", Username = "newuser", Phone = "0123", RoleSlug = "admin" };
        var command = new UpdateUserCommand(userId, request);
        var existingUser = new User { Id = userId }; // Cùng ID nên sẽ pass check duplicate
        var role = new Role { Id = Guid.NewGuid(), Slug = "admin" };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username)).ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(x => x.GetByPhoneAsync(request.Phone)).ReturnsAsync(existingUser);
        _roleRepositoryMock.Setup(x => x.GetBySlugAsync(request.RoleSlug)).ReturnsAsync(role);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(existingUser);
        _mapperMock.Setup(m => m.Map<UserRep>(It.IsAny<User>())).Returns(new UserRep());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        existingUser.RoleId.Should().Be(role.Id);
        _userRepositoryMock.Verify(x => x.UpdateAsync(existingUser), Times.Once);
    }

    // 2. LỖI TRÙNG EMAIL (Khác ID)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenEmailExistsForDifferentUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserReq { Email = "dup@test.com" };
        var command = new UpdateUserCommand(userId, request);
        var otherUser = new User { Id = Guid.NewGuid() }; // Khác ID

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(otherUser);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ConflictDuplicateException>()
            .WithMessage($"Email '{request.Email}' đã tồn tại");
    }

    // 3. LỖI TRÙNG USERNAME (Khác ID)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenUsernameExistsForDifferentUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserReq { Username = "dupuser" };
        var command = new UpdateUserCommand(userId, request);
        var otherUser = new User { Id = Guid.NewGuid() };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string?>())).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username)).ReturnsAsync(otherUser);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ConflictDuplicateException>()
            .WithMessage($"Username '{request.Username}' đã tồn tại");
    }

    // 4. LỖI TRÙNG SỐ ĐIỆN THOẠI (Khác ID)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenPhoneExistsForDifferentUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserReq { Phone = "999" };
        var command = new UpdateUserCommand(userId, request);
        var otherUser = new User { Id = Guid.NewGuid() };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string?>())).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<string?>())).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(x => x.GetByPhoneAsync(request.Phone)).ReturnsAsync(otherUser);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ConflictDuplicateException>()
            .WithMessage($"Số điện thoại '{request.Phone}' đã tồn tại");
    }

    // 5. LỖI KHÔNG TÌM THẤY ROLE
    [Fact]
    public async Task Handle_ShouldThrowException_WhenRoleNotFound()
    {
        // Arrange
        var request = new UpdateUserReq { RoleSlug = "non-existent" };
        var command = new UpdateUserCommand(Guid.NewGuid(), request);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string?>())).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<string?>())).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(x => x.GetByPhoneAsync(It.IsAny<string?>())).ReturnsAsync((User?)null);
        _roleRepositoryMock.Setup(x => x.GetBySlugAsync(request.RoleSlug)).ReturnsAsync((Role?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage($"Không tìm thấy vai trò '{request.RoleSlug}'");
    }

    // 6. LỖI KHÔNG TÌM THẤY USER (User Not Found)
    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserReq { RoleSlug = "admin" };
        var command = new UpdateUserCommand(userId, request);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string?>())).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<string?>())).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(x => x.GetByPhoneAsync(It.IsAny<string?>())).ReturnsAsync((User?)null);
        _roleRepositoryMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>())).ReturnsAsync(new Role());
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<BusinessException>();
    }
}