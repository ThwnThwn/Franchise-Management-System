using FluentAssertions;
using IdentityService.Application.Constants;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace IdentityService.Application.Tests.Features.Permissions;

public class PermissionSyncServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IPermissionRepository> _permRepoMock;
    private readonly Mock<IPermissionCache> _permCacheMock;
    private readonly PermissionSyncService _service;

    public PermissionSyncServiceTests()
    {
        _roleRepoMock = new Mock<IRoleRepository>();
        _permRepoMock = new Mock<IPermissionRepository>();
        _permCacheMock = new Mock<IPermissionCache>();
        _service = new PermissionSyncService(
            _roleRepoMock.Object,
            _permRepoMock.Object,
            _permCacheMock.Object
        );
    }

    [Fact]
    public async Task SyncAllAsync_ShouldPerformFullSync_WhenCalled()
    {
        // --- 1. Arrange ---
        // Lấy danh sách mẫu từ AppPermissions (Code)
        var codePermissions = AppPermissions.AllPermissions.Take(2).ToList();
        // Giả lập DB: 1 quyền khớp, 1 quyền cũ cần xóa, và thiếu 1 quyền từ code
        var existingPerm = new Permission
        {
            Slug = codePermissions[0].Slug,
            Resource = "OldResource" // Để test case Update Resource
        };
        var obsoletePerm = new Permission { Id = Guid.NewGuid(), Slug = "obsolete.slug" };

        var dbPermissions = new List<Permission> { existingPerm, obsoletePerm };

        _permRepoMock.Setup(x => x.GetAllAsync(
            It.IsAny<Expression<Func<Permission, object>>[]>(), // tham số includes
            It.IsAny<Expression<Func<Permission, bool>>>()      // tham số predicate
        ))
        .ReturnsAsync(dbPermissions);

        // Giả lập Roles để sync cache
        var roles = new List<Role>
        {
            new Role
            {
                Slug = "admin",
                Permissions = new List<Permission> { new Permission { Slug = "p1" } }
            },
            new Role { Slug = "", Permissions = new List<Permission>() } // Case slug trống để phủ nhánh IF
        };
        _roleRepoMock.Setup(x => x.GetAllDetailsAsync()).ReturnsAsync(roles);

        // --- 2. Act ---
        await _service.SyncAllAsync();

        // --- 3. Assert ---

        // Kiểm tra AddRange: Phải thêm những quyền có trong code mà ko có trong DB
        _permRepoMock.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<Permission>>(list =>
            list.Any(p => p.Slug == codePermissions[1].Slug))), Times.Once);

        // Kiểm tra UpdateRange: Phải cập nhật Resource cho quyền existingPerm
        _permRepoMock.Verify(x => x.UpdateRangeAsync(It.Is<IEnumerable<Permission>>(list =>
            list.Any(p => p.Slug == existingPerm.Slug && p.Resource == codePermissions[0].Resource))), Times.Once);

        // Kiểm tra DeleteRange: Phải xóa obsoletePerm
        _permRepoMock.Verify(x => x.DeleteRangeAsync(It.Is<IEnumerable<Guid>>(list =>
            list.Contains(obsoletePerm.Id))), Times.Once);

        // Kiểm tra Cache
        _permCacheMock.Verify(x => x.ClearAllRolesCacheAsync(), Times.Once);
        _permCacheMock.Verify(x => x.SetRolePermissionsAsync("admin", It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public async Task SyncAllAsync_ShouldDoNothing_WhenDbAndCodeAreInSync()
    {
        // Arrange: Mọi thứ đã đồng bộ sẵn
        var codePermissions = AppPermissions.AllPermissions;
        var dbPermissions = codePermissions.Select(c => new Permission
        {
            Slug = c.Slug,
            Resource = c.Resource
        }).ToList();

        _permRepoMock.Setup(x => x.GetAllAsync(
            It.IsAny<Expression<Func<Permission, object>>[]>(), // tham số includes
            It.IsAny<Expression<Func<Permission, bool>>>()      // tham số predicate
        ))
        .ReturnsAsync(dbPermissions);
        _roleRepoMock.Setup(x => x.GetAllDetailsAsync()).ReturnsAsync(new List<Role>());

        // Act
        await _service.SyncAllAsync();

        // Assert: Không gọi các hàm thay đổi DB
        _permRepoMock.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<Permission>>()), Times.Never);
        _permRepoMock.Verify(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<Permission>>()), Times.Never);
        _permRepoMock.Verify(x => x.DeleteRangeAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }
}