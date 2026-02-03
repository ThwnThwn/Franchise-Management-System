using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ProductService.Application.Dtos;

namespace ProductService.Application.Interfaces
{
    public interface ICloudinaryService
    {
        Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file);
        Task DeleteImageAsync(string publicId);
    }
}
