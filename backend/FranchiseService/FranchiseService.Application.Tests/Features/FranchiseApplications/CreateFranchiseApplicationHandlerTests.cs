using FluentAssertions;
using FranchiseService.Application.Dtos.Image;
using FranchiseService.Application.Features.FranchiseApplications.Commands.CreateFranchiseApplication;
using FranchiseService.Application.Interfaces;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Models;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseApplications;

public class CreateFranchiseApplicationHandlerTests
{
    private readonly Mock<IFranchiseApplicationRepository> _appRepoMock = new();
    private readonly Mock<IImageService> _imageServiceMock = new();
    private readonly Mock<IGeocodingService> _geocodingServiceMock = new();

    [Fact]
    public async Task Handle_ShouldCreateApplicationWithUploadsAndGeocode()
    {
        FranchiseApplication? createdApplication = null;
        _appRepoMock
            .Setup(x => x.AddAsync(It.IsAny<FranchiseApplication>()))
            .Callback<FranchiseApplication>(app => createdApplication = app)
            .ReturnsAsync(Guid.NewGuid());

        _imageServiceMock
            .SetupSequence(x => x.UploadImageFromBytesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUploadResult { Url = "https://cdn.cafe.vn/land/arabica-1.jpg", PublicId = "land_arabica_1" })
            .ReturnsAsync(new ImageUploadResult { Url = "https://cdn.cafe.vn/land/robusta-2.jpg", PublicId = "land_robusta_2" })
            .ReturnsAsync(new ImageUploadResult { Url = "https://cdn.cafe.vn/store/suadac-front.jpg", PublicId = "store_suadac_front" });

        _geocodingServiceMock
            .Setup(x => x.GeocodeAsync("123 Duong Arabica, Quan 1"))
            .ReturnsAsync(new CoordinateDto
            {
                Latitude = 10.777,
                Longitude = 106.699
            });

        var command = new CreateFranchiseApplicationCommand
        {
            ManagerId = Guid.NewGuid(),
            FullName = "Nguyen Arabica",
            Email = "arabica.owner@cfms.vn",
            Phone = "0909000001",
            IdentityNumber = "079123456789",
            ProvinceId = "79",
            DistrictId = "760",
            WardId = "26734",
            StoreName = "Sua Da Arabica House",
            StoreAddress = "123 Duong Arabica, Quan 1",
            StoreSizeSqm = 42,
            LandCertificateFiles =
            [
                new FileUploadItem { Data = [1, 2, 3], FileName = "so-do-arabica.jpg", ContentType = "image/jpeg" },
                new FileUploadItem { Data = [4, 5, 6], FileName = "hop-dong-thue-robusta.jpg", ContentType = "image/jpeg" }
            ],
            StoreFrontFile = new FileUploadItem
            {
                Data = [9, 9, 9],
                FileName = "mat-bang-suadac.jpg",
                ContentType = "image/jpeg"
            }
        };

        var handler = new CreateFranchiseApplicationHandler(
            _appRepoMock.Object,
            _imageServiceMock.Object,
            _geocodingServiceMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        createdApplication.Should().NotBeNull();
        createdApplication!.LandCertificates.Should().BeEquivalentTo(
            "https://cdn.cafe.vn/land/arabica-1.jpg",
            "https://cdn.cafe.vn/land/robusta-2.jpg");
        createdApplication.StoreFrontUrl.Should().Be("https://cdn.cafe.vn/store/suadac-front.jpg");
        createdApplication.StoreFrontPublicId.Should().Be("store_suadac_front");
        createdApplication.StoreLatitude.Should().Be(10.777m);
        createdApplication.StoreLongitude.Should().Be(106.699m);

        _imageServiceMock.Verify(
            x => x.UploadImageFromBytesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(3));
        _geocodingServiceMock.Verify(x => x.GeocodeAsync(command.StoreAddress), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkipGeocode_WhenCoordinatesAlreadyProvided()
    {
        _appRepoMock
            .Setup(x => x.AddAsync(It.IsAny<FranchiseApplication>()))
            .ReturnsAsync(Guid.NewGuid());

        var command = new CreateFranchiseApplicationCommand
        {
            ManagerId = Guid.NewGuid(),
            FullName = "Tran Robusta",
            Email = "robusta.owner@cfms.vn",
            Phone = "0909000002",
            IdentityNumber = "079123456788",
            StoreName = "Robusta Signature",
            StoreAddress = "45 Pho Ca Phe",
            StoreLatitude = 10.1m,
            StoreLongitude = 106.1m,
            StoreSizeSqm = 30,
            LandCertificateFiles = [],
            StoreFrontFile = null
        };

        var handler = new CreateFranchiseApplicationHandler(
            _appRepoMock.Object,
            _imageServiceMock.Object,
            _geocodingServiceMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _geocodingServiceMock.Verify(x => x.GeocodeAsync(It.IsAny<string>()), Times.Never);
        _imageServiceMock.Verify(
            x => x.UploadImageFromBytesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldKeepNullCoordinates_WhenGeocodeReturnsNull()
    {
        FranchiseApplication? createdApplication = null;

        _appRepoMock
            .Setup(x => x.AddAsync(It.IsAny<FranchiseApplication>()))
            .Callback<FranchiseApplication>(app => createdApplication = app)
            .ReturnsAsync(Guid.NewGuid());

        _geocodingServiceMock
            .Setup(x => x.GeocodeAsync("99 Ngo Sua Dac"))
            .ReturnsAsync((CoordinateDto?)null);

        var command = new CreateFranchiseApplicationCommand
        {
            ManagerId = Guid.NewGuid(),
            FullName = "Le Sua Dac",
            Email = "suadac.owner@cfms.vn",
            Phone = "0909000003",
            IdentityNumber = "079123456787",
            StoreName = "Sua Dac Lab",
            StoreAddress = "99 Ngo Sua Dac",
            StoreLatitude = null,
            StoreLongitude = null,
            StoreSizeSqm = 55
        };

        var handler = new CreateFranchiseApplicationHandler(
            _appRepoMock.Object,
            _imageServiceMock.Object,
            _geocodingServiceMock.Object);

        await handler.Handle(command, CancellationToken.None);

        createdApplication.Should().NotBeNull();
        createdApplication!.StoreLatitude.Should().BeNull();
        createdApplication.StoreLongitude.Should().BeNull();
        createdApplication.LandCertificates.Should().BeNull();
        createdApplication.StoreFrontUrl.Should().BeNull();
    }
}
