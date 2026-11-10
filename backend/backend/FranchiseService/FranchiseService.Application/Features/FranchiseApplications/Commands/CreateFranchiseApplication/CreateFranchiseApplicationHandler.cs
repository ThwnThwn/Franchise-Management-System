using FranchiseService.Application.Interfaces;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using MediatR;

namespace FranchiseService.Application.Features.FranchiseApplications.Commands.CreateFranchiseApplication;

public class CreateFranchiseApplicationHandler : IRequestHandler<CreateFranchiseApplicationCommand, Guid>
{
    private readonly IFranchiseApplicationRepository _appRepo;
    private readonly IImageService _imageService;
    private readonly IGeocodingService _geocodingService;

    public CreateFranchiseApplicationHandler(
        IFranchiseApplicationRepository appRepo,
        IImageService imageService,
        IGeocodingService geocodingService)
    {
        _appRepo = appRepo;
        _imageService = imageService;
        _geocodingService = geocodingService;
    }

    public async Task<Guid> Handle(CreateFranchiseApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Upload nhiều ảnh chứng nhận đất đai → TEXT[] trong DB
        var landCertUrls = new List<string>();
        if (request.LandCertificateFiles?.Count > 0)
        {
            foreach (var file in request.LandCertificateFiles)
            {
                var result = await _imageService.UploadImageFromBytesAsync(
                    file.Data, file.FileName, file.ContentType);
                landCertUrls.Add(result.Url);
            }
        }

        // 2. Upload ảnh mặt bằng
        string? storeFrontUrl = null;
        string? storeFrontPublicId = null;
        if (request.StoreFrontFile != null && request.StoreFrontFile.Data.Length > 0)
        {
            var result = await _imageService.UploadImageFromBytesAsync(
                request.StoreFrontFile.Data,
                request.StoreFrontFile.FileName,
                request.StoreFrontFile.ContentType);
            storeFrontUrl = result.Url;
            storeFrontPublicId = result.PublicId;
        }

        // 3. Xử lý Geocoding (Tự tự lấy vĩ độ/kinh độ từ địa chỉ nếu Front-end không gửi)
        if (request.StoreLongitude == null || request.StoreLatitude == null || request.StoreLongitude == 0 || request.StoreLatitude == 0)
        {
            var geoResult = await _geocodingService.GeocodeAsync(request.StoreAddress);
            if (geoResult != null)
            {
                request.StoreLatitude = (decimal)geoResult.Latitude;
                request.StoreLongitude = (decimal)geoResult.Longitude;
            }
        }

        // 3. Tạo entity
        var application = new FranchiseApplication
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.Empty,
            ManagerId = request.ManagerId,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            IdentityNumber = request.IdentityNumber,
            ProvinceId = request.ProvinceId,
            DistrictId = request.DistrictId,
            WardId = request.WardId,
            StoreName = request.StoreName,
            StoreAddress = request.StoreAddress,
            StoreLatitude = request.StoreLatitude,
            StoreLongitude = request.StoreLongitude,
            StoreSizeSqm = request.StoreSizeSqm,
            LandCertificates = landCertUrls.Count > 0 ? landCertUrls.ToArray() : null,
            StoreFrontUrl = storeFrontUrl,
            StoreFrontPublicId = storeFrontPublicId,
            Status = FranchiseApplicationStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _appRepo.AddAsync(application);
        return application.Id;
    }
}