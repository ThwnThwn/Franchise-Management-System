using FluentAssertions;
using FranchiseService.Application.Dtos.Image;
using FranchiseService.Application.Features.FranchiseApplications.Commands.UploadApplicationImage;
using FranchiseService.Application.Interfaces;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Models;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseApplications;

public class UploadApplicationImageHandlerTests
{
    private readonly Mock<IFranchiseApplicationRepository> _appRepoMock = new();
    private readonly Mock<IImageService> _imageServiceMock = new();

    [Fact]
    public async Task Handle_ShouldThrow_WhenApplicationNotFound()
    {
        var command = new UploadApplicationImageCommand(
            Guid.NewGuid(),
            [1, 2, 3],
            "arabica-front.jpg",
            "image/jpeg",
            "STORE_FRONT");

        _appRepoMock
            .Setup(x => x.GetByIdAsync(command.ApplicationId))
            .ReturnsAsync((FranchiseApplication?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Không tìm thấy hồ sơ*");
    }

    [Fact]
    public async Task Handle_ShouldUpdateStoreFrontAndDeleteOldImage_WhenTypeIsStoreFront()
    {
        var application = new FranchiseApplication
        {
            Id = Guid.NewGuid(),
            StoreFrontPublicId = "old_suadac_front",
            LandCertificates = ["https://cdn.cafe.vn/land/so-do-1.jpg"],
            Contracts = new List<FranchiseContract>()
        };

        _appRepoMock
            .Setup(x => x.GetByIdAsync(application.Id))
            .ReturnsAsync(application);

        _appRepoMock
            .Setup(x => x.UpdateAsync(application))
            .ReturnsAsync(1);

        _imageServiceMock
            .Setup(x => x.UploadImageFromBytesAsync(It.IsAny<byte[]>(), "new-front.jpg", "image/jpeg"))
            .ReturnsAsync(new ImageUploadResult
            {
                Url = "https://cdn.cafe.vn/store/new-front.jpg",
                PublicId = "new_front_pub"
            });

        var command = new UploadApplicationImageCommand(
            application.Id,
            [9, 9],
            "new-front.jpg",
            "image/jpeg",
            "store_front");

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.ApplicationId.Should().Be(application.Id);
        result.ImageUrl.Should().Be("https://cdn.cafe.vn/store/new-front.jpg");
        result.ImagePublicId.Should().Be("new_front_pub");

        application.StoreFrontUrl.Should().Be("https://cdn.cafe.vn/store/new-front.jpg");
        application.StoreFrontPublicId.Should().Be("new_front_pub");

        _appRepoMock.Verify(x => x.UpdateAsync(application), Times.Once);
        _imageServiceMock.Verify(x => x.DeleteImageAsync("old_suadac_front"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAppendLandCertificate_WhenTypeIsLandCertificate()
    {
        var application = new FranchiseApplication
        {
            Id = Guid.NewGuid(),
            LandCertificates = ["https://cdn.cafe.vn/land/arabica-a.jpg"],
            Contracts = new List<FranchiseContract>()
        };

        _appRepoMock
            .Setup(x => x.GetByIdAsync(application.Id))
            .ReturnsAsync(application);

        _appRepoMock
            .Setup(x => x.UpdateAsync(application))
            .ReturnsAsync(1);

        _imageServiceMock
            .Setup(x => x.UploadImageFromBytesAsync(It.IsAny<byte[]>(), "land-robusta.jpg", "image/jpeg"))
            .ReturnsAsync(new ImageUploadResult
            {
                Url = "https://cdn.cafe.vn/land/robusta-b.jpg",
                PublicId = "land_robusta_b"
            });

        var command = new UploadApplicationImageCommand(
            application.Id,
            [7, 7],
            "land-robusta.jpg",
            "image/jpeg",
            "LAND_CERTIFICATE");

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.ImageUrl.Should().Be("https://cdn.cafe.vn/land/robusta-b.jpg");
        application.LandCertificates.Should().BeEquivalentTo(
            "https://cdn.cafe.vn/land/arabica-a.jpg",
            "https://cdn.cafe.vn/land/robusta-b.jpg");

        _imageServiceMock.Verify(x => x.DeleteImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenImageTypeIsInvalid()
    {
        var application = new FranchiseApplication
        {
            Id = Guid.NewGuid(),
            Contracts = new List<FranchiseContract>()
        };

        _appRepoMock
            .Setup(x => x.GetByIdAsync(application.Id))
            .ReturnsAsync(application);

        _imageServiceMock
            .Setup(x => x.UploadImageFromBytesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUploadResult
            {
                Url = "https://cdn.cafe.vn/tmp/tmp.jpg",
                PublicId = "tmp_public"
            });

        var command = new UploadApplicationImageCommand(
            application.Id,
            [5],
            "tmp.jpg",
            "image/jpeg",
            "MENU_BANNER");

        var handler = CreateHandler();
        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*không hợp lệ*");
    }

    private UploadApplicationImageHandler CreateHandler()
    {
        return new UploadApplicationImageHandler(
            _appRepoMock.Object,
            _imageServiceMock.Object);
    }
}


