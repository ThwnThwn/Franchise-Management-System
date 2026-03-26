using Moq;
using Xunit;
using FluentAssertions;
using IdentityService.Application.Features.Permission.Commands.UpdateInfo;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Tests.Features.Permissions;

public class UpdatePermissionInfoHandlerTest
{
    private readonly Mock<IPermissionRepository> _permRepoMock;
    private readonly UpdatePermissionInfoHandler _handler;

    public UpdatePermissionInfoHandlerTest()
    {
        _permRepoMock = new Mock<IPermissionRepository>();
        _handler = new UpdatePermissionInfoHandler(_permRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenPermissionNotFound()
    {
        // Arrange
        var command = new UpdatePermissionInfoCommand { Id = Guid.NewGuid() };
        _permRepoMock.Setup(x => x.GetByIdAsync(command.Id))
            .ReturnsAsync((Permission?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Không tìm thấy perrmission này");
    }

    [Fact]
    public async Task Handle_ShouldUpdateAllFields_WhenRequestHasValues()
    {
        // Arrange
        var permId = Guid.NewGuid();
        var existingPerm = new Permission { Id = permId, Name = "Old Name", Description = "Old Desc" };
        var command = new UpdatePermissionInfoCommand
        {
            Id = permId,
            Name = "New Name",
            Description = "New Desc"
        };

        _permRepoMock.Setup(x => x.GetByIdAsync(permId)).ReturnsAsync(existingPerm);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingPerm.Name.Should().Be("New Name");
        existingPerm.Description.Should().Be("New Desc");
        _permRepoMock.Verify(x => x.UpdateAsync(existingPerm), Times.Once);
        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_ShouldKeepOldValues_WhenRequestFieldsAreNullOrEmpty()
    {
        // Arrange
        var permId = Guid.NewGuid();
        var existingPerm = new Permission { Id = permId, Name = "Old Name", Description = "Old Desc" };

        // Test trường hợp Name rỗng và Description null
        var command = new UpdatePermissionInfoCommand
        {
            Id = permId,
            Name = "",
            Description = null
        };

        _permRepoMock.Setup(x => x.GetByIdAsync(permId)).ReturnsAsync(existingPerm);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingPerm.Name.Should().Be("Old Name");
        existingPerm.Description.Should().Be("Old Desc");
        _permRepoMock.Verify(x => x.UpdateAsync(existingPerm), Times.Once);
    }
}