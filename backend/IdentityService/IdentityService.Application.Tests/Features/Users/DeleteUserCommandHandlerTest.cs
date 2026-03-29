using FluentAssertions;
using IdentityService.Application.Features.Users.Commands.DeleteUser;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Validators.Users;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;
using FluentValidation.TestHelper;

namespace IdentityService.Application.Tests.Features.Users;

public class DeleteUserCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly DeleteUserCommandHandler _handler;
    private readonly DeleteUserCommandValidator _validator;

    public DeleteUserCommandHandlerTest()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new DeleteUserCommandHandler(_userRepositoryMock.Object);
        _validator = new DeleteUserCommandValidator();
    }
    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        // Arrange: Khởi tạo command với Guid rỗng
        var command = new DeleteUserCommand(Guid.Empty);

        // Act: Thực hiện validate
        var result = _validator.TestValidate(command);

        // Assert: Kiểm tra lỗi tồn tại với đúng message
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("User Id is required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Id_Is_Valid()
    {
        // Arrange: Khởi tạo command với một Guid hợp lệ
        var command = new DeleteUserCommand(Guid.NewGuid());

        // Act: Thực hiện validate
        var result = _validator.TestValidate(command);

        // Assert: Đảm bảo không có lỗi validation nào
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        // Đảm bảo hàm Delete không bao giờ được gọi nếu không tìm thấy User
        _userRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenUserIsDeletedSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var existingUser = new User { Id = userId, Username = "testuser" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        // Verify việc gọi hàm Delete đúng với Id của User đó
        _userRepositoryMock.Verify(x => x.DeleteAsync(userId), Times.Once);
    }
}