using FranchiseService.Application.Dtos.Image;
using Microsoft.AspNetCore.Http;

namespace FranchiseService.Application.Interfaces
{
    public interface IImageService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file);

        Task<ImageUploadResult> UploadImageFromBytesAsync(byte[] imageData, string fileName, string contentType);

        Task DeleteImageAsync(string publicId);
    }
}