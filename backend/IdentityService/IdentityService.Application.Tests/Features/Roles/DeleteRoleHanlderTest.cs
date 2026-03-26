using FluentAssertions;
using IdentityService.Application.Features.Roles.Commands.DeleteRole;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Roles;

public class DeleteRoleCommandHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly DeleteRoleCommandHandler _handler;

    public DeleteRoleCommandHandlerTest()
    {
        _roleRepoMock = new Mock<IRoleRepository>();
        _handler = new DeleteRoleCommandHandler(_roleRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenRoleNotFound()
    {
        // Arrange: Giả lập repository trả về null (không tìm thấy Role)
        var roleId = Guid.NewGuid();
        _roleRepoMock.Setup(x => x.GetByIdAsync(roleId))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _handler.Handle(new DeleteRoleCommand(roleId), default);

        // Assert
        result.Should().BeFalse();
        // Đảm bảo không gọi hàm Delete nếu không tìm thấy Role
        _roleRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenRoleIsDeletedSuccessfully()
    {
        // Arrange: Giả lập tìm thấy Role
        var roleId = Guid.NewGuid();
        var role = new Role { Id = roleId, Name = "Admin" };

        _roleRepoMock.Setup(x => x.GetByIdAsync(roleId))
            .ReturnsAsync(role);

        _roleRepoMock.Setup(x => x.DeleteAsync(roleId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(new DeleteRoleCommand(roleId), default);

        // Assert
        result.Should().BeTrue();
        // Xác nhận hàm Delete đã được gọi đúng ID
        _roleRepoMock.Verify(x => x.DeleteAsync(roleId), Times.Once);
    }
}