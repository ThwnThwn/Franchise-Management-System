using Moq;
using Xunit;
using FluentAssertions;
using Grpc.Core;
using IdentityService.Application.Features.Users.Commands.DeleteAvatar;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.ExternalServices;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Tests.Features.Users;

public class DeleteAvatarCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
    private readonly DeleteAvatarCommandHandler _handler;

    public DeleteAvatarCommandHandlerTest()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _cloudinaryServiceMock = new Mock<ICloudinaryService>();
        _handler = new DeleteAvatarCommandHandler(
            _userRepositoryMock.Object,
            _cloudinaryServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _handler.Handle(new DeleteAvatarCommand(userId), CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.Status.StatusCode.Should().Be(StatusCode.NotFound);
        exception.Which.Status.Detail.Should().Be("User not found");
    }

    [Theory]
    [InlineData(null, "public-id")]
    [InlineData("http://avatar.com", null)]
    [InlineData("", "")]
    public async Task Handle_ShouldThrowNotFound_WhenUserHasNoAvatar(string? url, string? publicId)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, AvatarUrl = url, AvatarPublicId = publicId };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var act = () => _handler.Handle(new DeleteAvatarCommand(userId), CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.Status.Detail.Should().Be("User does not have an avatar to delete");

        // Đảm bảo không gọi Cloudinary
        _cloudinaryServiceMock.Verify(x => x.DeleteImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteSuccessfully_WhenUserHasAvatar()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var publicId = "avatars/user123";
        var user = new User
        {
            Id = userId,
            AvatarUrl = "https://cloudinary.com/v1/image.jpg",
            AvatarPublicId = publicId
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _cloudinaryServiceMock.Setup(x => x.DeleteImageAsync(publicId)).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(new DeleteAvatarCommand(userId), CancellationToken.None);

        // Assert
        result.Should().Be("Delete avatar successfully");
        user.AvatarUrl.Should().BeNull();
        user.AvatarPublicId.Should().BeNull();

        // Verify calls
        _cloudinaryServiceMock.Verify(x => x.DeleteImageAsync(publicId), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}