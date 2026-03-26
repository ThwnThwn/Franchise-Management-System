using AutoMapper;
using FluentAssertions;
using IdentityService.Application.Dtos.Cloudinary;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Features.Users.Commands.UploadAvatar;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using Moq;
using Xunit;

namespace IdentityService.Application.Tests.Features.Users;

public class UploadAvatarCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UploadAvatarCommandHandler _handler;

    public UploadAvatarCommandHandlerTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _cloudinaryMock = new Mock<ICloudinaryService>();
        _mapperMock = new Mock<IMapper>();
        _handler = new UploadAvatarCommandHandler(
            _userRepoMock.Object,
            _cloudinaryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(new UploadAvatarCommand(userId, []), default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task Handle_ShouldDeleteOldAvatar_WhenAvatarPublicIdExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldPublicId = "old_avatar_id";
        var user = new User { Id = userId, AvatarPublicId = oldPublicId };
        var imageData = new byte[] { 1, 2, 3 };

        var uploadResult = new CloudinaryUploadResult
        {
            Url = "http://new-url.com",
            PublicId = "new_id"
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _cloudinaryMock.Setup(x => x.UploadImageFromBytesAsync(It.IsAny<byte[]>()))
            .ReturnsAsync(uploadResult);

        // Act
        await _handler.Handle(new UploadAvatarCommand(userId, imageData), default);

        // Assert
        // Verify nhánh xóa avatar cũ được thực thi
        _cloudinaryMock.Verify(x => x.DeleteImageAsync(oldPublicId), Times.Once);
        _cloudinaryMock.Verify(x => x.UploadImageFromBytesAsync(imageData), Times.Once);
        _userRepoMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteAnything_WhenAvatarPublicIdIsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, AvatarPublicId = null }; // Hoặc ""
        var imageData = new byte[] { 4, 5, 6 };
        var uploadResult = new CloudinaryUploadResult
        {
            Url = "http://new-url.com",
            PublicId = "new_id"
        };
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _cloudinaryMock.Setup(x => x.UploadImageFromBytesAsync(imageData)).ReturnsAsync(uploadResult);

        // Act
        await _handler.Handle(new UploadAvatarCommand(userId, imageData), default);

        // Assert
        // Verify nhánh xóa KHÔNG được thực thi
        _cloudinaryMock.Verify(x => x.DeleteImageAsync(It.IsAny<string>()), Times.Never);
        _cloudinaryMock.Verify(x => x.UploadImageFromBytesAsync(imageData), Times.Once);

        user.AvatarUrl.Should().Be(uploadResult.Url);
        user.AvatarPublicId.Should().Be(uploadResult.PublicId);
    }
}