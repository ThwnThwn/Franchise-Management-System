using FranchiseService.Application.Interfaces;
using FranchiseService.Application.Repositories;
using MediatR;

namespace FranchiseService.Application.Features.FranchiseApplications.Commands.UploadApplicationImage;

public class UploadApplicationImageHandler : IRequestHandler<UploadApplicationImageCommand, UploadImageCommandResponse>
{
    private readonly IFranchiseApplicationRepository _appRepo;
    private readonly IImageService _imageService;

    public UploadApplicationImageHandler(
        IFranchiseApplicationRepository appRepo,
        IImageService imageService)
    {
        _appRepo = appRepo;
        _imageService = imageService;
    }

    public async Task<UploadImageCommandResponse> Handle(
        UploadApplicationImageCommand request,
        CancellationToken cancellationToken)
    {
        var application = await _appRepo.GetByIdAsync(request.ApplicationId);
        if (application == null)
        {
            throw new Exception("Không tìm thấy hồ sơ để cập nhật ảnh.");
        }

        var oldPublicId = application.StoreFrontPublicId;

        // Tải ảnh lên
        var uploadResult = await _imageService.UploadImageFromBytesAsync(
            request.ImageData,
            request.FileName,
            request.ContentType);

        if (request.ImageType?.ToUpper() == "STORE_FRONT")
        {
            application.StoreFrontUrl = uploadResult.Url;
            application.StoreFrontPublicId = uploadResult.PublicId;

            await _appRepo.UpdateAsync(application);

            // Xóa ảnh cũ nếu có
            if (!string.IsNullOrWhiteSpace(oldPublicId))
            {
                await _imageService.DeleteImageAsync(oldPublicId);
            }
        }
        else if (request.ImageType?.ToUpper() == "LAND_CERTIFICATE")
        {
            var currentCerts = application.LandCertificates?.ToList() ?? new List<string>();
            currentCerts.Add(uploadResult.Url);
            application.LandCertificates = currentCerts.ToArray();

            await _appRepo.UpdateAsync(application);
        }
        else
        {
            throw new Exception("Loại hình ảnh không hợp lệ. Phải là STORE_FRONT hoặc LAND_CERTIFICATE.");
        }

        return new UploadImageCommandResponse(
            ApplicationId: application.Id,
            ImageUrl: uploadResult.Url,
            ImagePublicId: uploadResult.PublicId
        );
    }
}
