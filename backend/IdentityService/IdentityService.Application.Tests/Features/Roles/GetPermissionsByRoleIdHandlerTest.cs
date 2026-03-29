using Moq;
using Xunit;
using FluentAssertions;
using IdentityService.Application.Features.Roles.Queries.GetPermissionsByRoleId;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using IdentityService.Application.Dtos.Role.Respone;

namespace IdentityService.Application.Tests.Features.Roles;

public class GetPermissionsByRoleIdHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly GetPermissionsByRoleIdHandler _handler;

    public GetPermissionsByRoleIdHandlerTest()
    {
        _roleRepoMock = new Mock<IRoleRepository>();
        _handler = new GetPermissionsByRoleIdHandler(_roleRepoMock.Object);
    }

    // 1. BRANCH: role == null (Ném Exception)
    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenRoleDoesNotExist()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleRepoMock.Setup(x => x.GetByIdDetailsAsync(roleId))
            .ReturnsAsync((Role?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new GetPermissionsByRoleIdQuery { Id = roleId }, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Không tìm thấy role này");
    }

    // 2. BRANCH: Thành công và logic GroupBy
    [Fact]
    public async Task Handle_ShouldReturnGroupedPermissions_WhenRoleExists()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissions = new List<Permission>
        {
            new Permission { Id = Guid.NewGuid(), Resource = "User", Slug = "user.create", Name = "Create User" },
            new Permission { Id = Guid.NewGuid(), Resource = "User", Slug = "user.view", Name = "View User" },
            new Permission { Id = Guid.NewGuid(), Resource = "Role", Slug = "role.view", Name = "View Role", Description = "Desc" }
        };

        var role = new Role
        {
            Id = roleId,
            Permissions = permissions
        };

        _roleRepoMock.Setup(x => x.GetByIdDetailsAsync(roleId))
            .ReturnsAsync(role);

        // Act
        var result = await _handler.Handle(new GetPermissionsByRoleIdQuery { Id = roleId }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var list = result.ToList();

        // Kiểm tra Grouping (2 groups: User và Role)
        list.Should().HaveCount(2);

        var userGroup = list.First(g => g.Resource == "User");
        userGroup.Permissions.Should().HaveCount(2);

        var roleGroup = list.First(g => g.Resource == "Role");
        roleGroup.Permissions.First().Description.Should().Be("Desc");
    }

    // 3. BRANCH: Role tồn tại nhưng danh sách Permission rỗng
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenRoleHasNoPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new Role { Id = roleId, Permissions = new List<Permission>() };

        _roleRepoMock.Setup(x => x.GetByIdDetailsAsync(roleId))
            .ReturnsAsync(role);

        // Act
        var result = await _handler.Handle(new GetPermissionsByRoleIdQuery { Id = roleId }, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}