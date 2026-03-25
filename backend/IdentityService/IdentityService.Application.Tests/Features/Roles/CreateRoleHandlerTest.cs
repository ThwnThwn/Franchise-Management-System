using Moq;
using Xunit;
using FluentAssertions;
using AutoMapper;
using IdentityService.Application.Features.Roles.Commands.CreateRole;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Dtos.Role.Request;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Domain.Entities;
using IdentityService.Application.AppExceptions;

namespace IdentityService.Application.Tests.Features.Roles;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateRoleCommandHandler _handler;

    public CreateRoleCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateRoleCommandHandler(_roleRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateRole_WhenRoleDoesNotExist()
    {
        // 1. Arrange
        var request = new CreateRoleReq
        {
            Name = "Admin",
            Slug = "admin",
            Description = "Quan tri vien he thong"
        };
        var command = new CreateRoleCommand(request);
        var roleEntity = new Role { Name = request.Name, Slug = request.Slug };
        var roleResponse = new RoleRep { Id = Guid.NewGuid(), Name = request.Name };

        // Mock: Tra ve null (chua ton tai role)
        _roleRepositoryMock.Setup(x => x.GetByNameAsync(request.Name))
            .ReturnsAsync((Role?)null);

        // Mock: Mapping tu Request sang Entity
        _mapperMock.Setup(m => m.Map<Role>(request))
            .Returns(roleEntity);

        // Mock: Mapping tu Entity sang Response
        _mapperMock.Setup(m => m.Map<RoleRep>(roleEntity))
            .Returns(roleResponse);

        // 2. Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // 3. Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);

        // Kiem tra xem timestamps co duoc set khong
        roleEntity.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        // Xac nhan Repository thuc su duoc goi AddAsync
        _roleRepositoryMock.Verify(x => x.AddAsync(roleEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowDbUpdateException_WhenRoleAlreadyExists()
    {
        // 1. Arrange
        var request = new CreateRoleReq { Name = "ExistingRole" };
        var command = new CreateRoleCommand(request);
        var existingRole = new Role { Name = "ExistingRole" };

        _roleRepositoryMock.Setup(x => x.GetByNameAsync(request.Name))
            .ReturnsAsync(existingRole);

        // 2. Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<DbUpdateException>()
            .WithMessage($"Role '{request.Name}' already exists");

        // Đảm bảo không gọi lệnh Add khi có lỗi
        _roleRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Role>()), Times.Never);
    }
}