using Moq;
using Xunit;
using FluentAssertions;
using IdentityService.Application.Features.Roles.Queries.GetPermissionsBySlug;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Persistences.Caches;

namespace IdentityService.Application.Tests.Features.Roles;

public class GetPermissionsBySlugHandlerTest
{
    private readonly Mock<IPermissionRepository> _permRepoMock;
    private readonly Mock<IPermissionCache> _permCacheMock;
    private readonly GetPermissionsBySlugHandler _handler;

    public GetPermissionsBySlugHandlerTest()
    {
        _permRepoMock = new Mock<IPermissionRepository>();
        _permCacheMock = new Mock<IPermissionCache>();
        _handler = new GetPermissionsBySlugHandler(_permRepoMock.Object, _permCacheMock.Object);
    }

    // 1. BRANCH: permissions != null (Có dữ liệu)
    [Fact]
    public async Task Handle_ShouldReturnPermissionsAndSetCache_WhenPermissionsExist()
    {
        // Arrange
        var roleSlug = "admin";
        var query = new GetPermissionsBySlugQuery { RoleSlug = roleSlug };
        var permissionSlugs = new List<string> { "user.create", "user.view" };

        _permRepoMock.Setup(x => x.GetSlugsByRoleSlugAsync(roleSlug, true))
            .ReturnsAsync(permissionSlugs);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(permissionSlugs);
        // Kiểm tra nhánh ghi vào Cache được thực thi
        _permCacheMock.Verify(x => x.SetRolePermissionsAsync(roleSlug, permissionSlugs), Times.Once);
    }

    // 2. BRANCH: permissions != null (List rỗng - Chống Cache Penetration)
    [Fact]
    public async Task Handle_ShouldReturnEmptyListAndSetCache_WhenPermissionsListIsEmpty()
    {
        // Arrange
        var roleSlug = "guest";
        var query = new GetPermissionsBySlugQuery { RoleSlug = roleSlug };
        var emptyList = new List<string>();

        _permRepoMock.Setup(x => x.GetSlugsByRoleSlugAsync(roleSlug, true))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _permCacheMock.Verify(x => x.SetRolePermissionsAsync(roleSlug, emptyList), Times.Once);
    }

    // 3. BRANCH: permissions == null (Trường hợp Repository trả về null)
    [Fact]
    public async Task Handle_ShouldReturnEmptyListAndNotSetCache_WhenPermissionsIsNull()
    {
        // Arrange
        var roleSlug = "unknown";
        var query = new GetPermissionsBySlugQuery { RoleSlug = roleSlug };

        _permRepoMock.Setup(x => x.GetSlugsByRoleSlugAsync(roleSlug, true))
            .ReturnsAsync((IEnumerable<string>)null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty(); // Kiểm tra toán tử ?? []

        // Kiểm tra nhánh cache KHÔNG được thực thi
        _permCacheMock.Verify(x => x.SetRolePermissionsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }
}