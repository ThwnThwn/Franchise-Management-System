using Microsoft.AspNetCore.Http;
using ProductService.Application.Dtos;

namespace ProductService.Application.Interfaces
{
    public interface ICloudinaryService
    {
        Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file);

        Task<CloudinaryUploadResult> UploadImageFromBytesAsync(byte[] imageData, string fileName, string contentType);

        Task DeleteImageAsync(string publicId);
    }
}