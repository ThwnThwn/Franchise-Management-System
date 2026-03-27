using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Dtos.Role.Request;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Application.Features.Roles.Commands.CreateRole;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Validators.Roles;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Roles;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateRoleCommandHandler _handler;
    private readonly CreateRoleCommandValidator _validator;

    public CreateRoleCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateRoleCommandHandler(_roleRepositoryMock.Object, _mapperMock.Object);
        _validator = new CreateRoleCommandValidator();
    }
    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var request = new CreateRoleReq { Name = "" };
        var command = new CreateRoleCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
              .WithErrorMessage("Role name is required");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
    {
        // Arrange: Tạo chuỗi dài 51 ký tự
        var longName = new string('A', 51);
        var request = new CreateRoleReq { Name = longName };
        var command = new CreateRoleCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        // Arrange
        var request = new CreateRoleReq { Name = "Admin" };
        var command = new CreateRoleCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
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
    // 3. KIỂM TRA TÍNH TOÀN VẸN CỦA DỮ LIỆU (Branch Coverage cho logic gán Timestamps)
    [Fact]
    public async Task Handle_ShouldSetTimestamps_WhenCreatingNewRole()
    {
        // Arrange
        var request = new CreateRoleReq { Name = "Editor", Slug = "editor" };
        var command = new CreateRoleCommand(request);
        var roleEntity = new Role(); // Entity trống để kiểm tra việc gán giá trị sau map

        _roleRepositoryMock.Setup(x => x.GetByNameAsync(request.Name)).ReturnsAsync((Role?)null);
        _mapperMock.Setup(m => m.Map<Role>(request)).Returns(roleEntity);
        _mapperMock.Setup(m => m.Map<RoleRep>(roleEntity)).Returns(new RoleRep());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Đảm bảo cả 2 mốc thời gian đều được khởi tạo và gần với hiện tại
        roleEntity.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        roleEntity.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));

        _roleRepositoryMock.Verify(x => x.AddAsync(It.Is<Role>(r =>
            r.CreatedAt != default && r.UpdatedAt != default)), Times.Once);
    }
}