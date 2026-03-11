using Microsoft.AspNetCore.Http;
using SalesService.Application.Dtos;

namespace SalesService.Application.Interfaces
{
    public interface IImageService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file);

        Task<ImageUploadResult> UploadImageFromBytesAsync(byte[] imageData, string fileName, string contentType);

        Task DeleteImageAsync(string publicId);
    }
}