using Moq;
using Xunit;
using FluentAssertions;
using IdentityService.Application.Features.Roles.Commands.UpdateRolePermissions;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Tests.Features.Roles;

public class UpdateRolePermissionsHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IPermissionRepository> _permRepoMock;
    private readonly Mock<IPermissionCache> _permCacheMock;
    private readonly UpdateRolePermissionsHandler _handler;

    public UpdateRolePermissionsHandlerTest()
    {
        _roleRepoMock = new Mock<IRoleRepository>();
        _permRepoMock = new Mock<IPermissionRepository>();
        _permCacheMock = new Mock<IPermissionCache>();
        _handler = new UpdateRolePermissionsHandler(
            _roleRepoMock.Object,
            _permRepoMock.Object,
            _permCacheMock.Object);
    }

    // 1. LỖI: KHÔNG TÌM THẤY ROLE (Branch: role == null)
    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenRoleDoesNotExist()
    {
        var command = new UpdateRolePermissionsCommand { Id = Guid.NewGuid() };
        _roleRepoMock.Setup(x => x.GetByIdDetailsAsync(command.Id)).ReturnsAsync((Role?)null);

        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    // 2. LOGIC GRANT: THÊM QUYỀN MỚI (Branch: Grant & existingPerm == null)
    [Fact]
    public async Task Handle_ShouldGrantPermission_WhenRoleDoesNotHaveIt()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permSlug = "user.create";
        var command = new UpdateRolePermissionsCommand
        {
            Id = roleId,
            PermissionsToGrant = [permSlug]
        };

        var role = new Role { Id = roleId, Slug = "admin", Permissions = new List<Permission>() };
        var permission = new Permission { Id = Guid.NewGuid(), Slug = permSlug };

        _roleRepoMock.Setup(x => x.GetByIdDetailsAsync(roleId)).ReturnsAsync(role);
        _permRepoMock.Setup(x => x.GetListBySlugsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync([permission]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        role.Permissions.Should().Contain(permission);
        _roleRepoMock.Verify(x => x.UpdateAsync(role), Times.Once);
    }

    // 3. LOGIC GRANT: QUYỀN ĐÃ TỒN TẠI (Branch: Grant & existingPerm != null)
    [Fact]
    public async Task Handle_ShouldNotAddDuplicate_WhenPermissionAlreadyExists()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permSlug = "user.view";
        var command = new UpdateRolePermissionsCommand { Id = roleId, PermissionsToGrant = [permSlug] };

        var permission = new Permission { Id = Guid.NewGuid(), Slug = permSlug };
        var role = new Role { Id = roleId, Slug = "admin", Permissions = [permission] };

        _roleRepoMock.Setup(x => x.GetByIdDetailsAsync(roleId)).ReturnsAsync(role);
        _permRepoMock.Setup(x => x.GetListBySlugsAsync(It.IsAny<List<string>>())).ReturnsAsync([permission]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        role.Permissions.Should().HaveCount(1); // Không thêm mới
    }

    // 4. LOGIC REVOKE: THU HỒI QUYỀN ĐANG CÓ (Branch: Revoke & existingPerm != null)
    [Fact]
    public async Task Handle_ShouldRevokePermission_WhenRoleHasIt()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permSlug = "user.delete";
        var command = new UpdateRolePermissionsCommand { Id = roleId, PermissionsToRevoke = [permSlug] };

        var permission = new Permission { Id = Guid.NewGuid(), Slug = permSlug };
        var role = new Role { Id = roleId, Slug = "admin", Permissions = [permission] };

        _roleRepoMock.Setup(x => x.GetByIdDetailsAsync(roleId)).ReturnsAsync(role);
        _permRepoMock.Setup(x => x.GetListBySlugsAsync(It.IsAny<List<string>>())).ReturnsAsync([permission]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        role.Permissions.Should().NotContain(permission);
    }

    // 5. LOGIC REVOKE: THU HỒI QUYỀN KHÔNG TỒN TẠI (Branch: Revoke & existingPerm == null)
    [Fact]
    public async Task Handle_ShouldDoNothing_WhenRevokingNonExistentPermission()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permSlug = "user.nonexistent";
        var command = new UpdateRolePermissionsCommand { Id = roleId, PermissionsToRevoke = [permSlug] };

        var role = new Role { Id = roleId, Slug = "admin", Permissions = [] };
        var permission = new Permission { Id = Guid.NewGuid(), Slug = permSlug };

        _roleRepoMock.Setup(x => x.GetByIdDetailsAsync(roleId)).ReturnsAsync(role);
        _permRepoMock.Setup(x => x.GetListBySlugsAsync(It.IsAny<List<string>>())).ReturnsAsync([permission]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        role.Permissions.Should().BeEmpty();
    }

    // 6. KIỂM TRA UPDATE CACHE (Logic sau vòng lặp)
    [Fact]
    public async Task Handle_ShouldUpdateCache_AfterProcessing()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new UpdateRolePermissionsCommand { Id = roleId };
        var role = new Role { Id = roleId, Slug = "admin", Permissions = [] };
        var activeSlugs = new List<string> { "p1", "p2" };

        _roleRepoMock.Setup(x => x.GetByIdDetailsAsync(roleId)).ReturnsAsync(role);
        _permRepoMock.Setup(x => x.GetSlugsByRoleSlugAsync(role.Slug, true)).ReturnsAsync(activeSlugs);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _permCacheMock.Verify(x => x.SetRolePermissionsAsync(role.Slug, activeSlugs), Times.Once);
    }
}