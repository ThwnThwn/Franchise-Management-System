using Moq;
using Xunit;
using FluentAssertions;
using AutoMapper;
using IdentityService.Application.Features.Roles.Queries.GetRoleById;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Tests.Features.Roles;

public class GetRoleByIdQueryHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetRoleByIdQueryHandler _handler;

    public GetRoleByIdQueryHandlerTest()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetRoleByIdQueryHandler(_roleRepositoryMock.Object, _mapperMock.Object);
    }

    // 1. BRANCH: role != null (Trường hợp tìm thấy Role)
    [Fact]
    public async Task Handle_ShouldReturnRoleRep_WhenRoleExists()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var query = new GetRoleByIdQuery(roleId);
        var role = new Role { Id = roleId, Name = "Admin" };
        var roleRep = new RoleRep { Name = "Admin" };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId))
            .ReturnsAsync(role);

        _mapperMock.Setup(m => m.Map<RoleRep>(role))
            .Returns(roleRep);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Admin");
        _roleRepositoryMock.Verify(x => x.GetByIdAsync(roleId), Times.Once);
        _mapperMock.Verify(m => m.Map<RoleRep>(role), Times.Once);
    }

    // 2. BRANCH: role == null (Trường hợp không tìm thấy Role)
    [Fact]
    public async Task Handle_ShouldReturnNull_WhenRoleDoesNotExist()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var query = new GetRoleByIdQuery(roleId);

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        // Đảm bảo Mapper không được gọi nếu role null để tránh phí tài nguyên
        _mapperMock.Verify(m => m.Map<RoleRep>(It.IsAny<Role>()), Times.Never);
    }
}