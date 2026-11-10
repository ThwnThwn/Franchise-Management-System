using IdentityService.Application.Dtos.Cloudinary;
using Microsoft.AspNetCore.Http;

namespace IdentityService.Application.ExternalServices
{
    public interface ICloudinaryService
    {
        Task<CloudinaryUploadResult> UploadImageFromBytesAsync(byte[] imageData);
        Task DeleteImageAsync(string publicId);
    }
}
