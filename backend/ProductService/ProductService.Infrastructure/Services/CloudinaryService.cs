using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ProductService.Infrastructure.Settings;
using ProductService.Application.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProductService.Application.Dtos;


namespace ProductService.Infrastructure.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;

            if (string.IsNullOrWhiteSpace(settings.CloudName) ||
                string.IsNullOrWhiteSpace(settings.ApiKey) ||
                string.IsNullOrWhiteSpace(settings.ApiSecret))
            {
                throw new InvalidOperationException(
                    "Cloudinary settings are not configured correctly. " +
                    "Please set CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY, CLOUDINARY_API_SECRET.");
            }

            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file)
        {
            // Kiểm tra file có hợp lệ không
            if (file == null || file.Length == 0)
                throw new ArgumentException("File upload is invalid");

            // Validate loại file (chỉ cho phép image)
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException($"Unsupported image extension: {fileExtension}");
            }

            // Đọc file vào MemoryStream để đảm bảo stream không bị dispose sớm
            // và Cloudinary có thể đọc được đúng cách
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // Upload ảnh vào folder products trong Cloudinary
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, memoryStream),
                Folder = "products"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // Kiểm tra xem upload có thành công không
            if (uploadResult == null || uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var errorMsg = uploadResult?.Error?.Message ?? "Unknown error";
                var status = uploadResult?.StatusCode.ToString() ?? "Unknown";
                throw new Exception($"Cloudinary upload failed: {errorMsg} (Status: {status})");
            }

            return new CloudinaryUploadResult
            {
                Url = uploadResult.SecureUrl.ToString(),
                PublicId = uploadResult.PublicId
            };
        }

        public async Task<CloudinaryUploadResult> UploadImageFromBytesAsync(byte[] imageData, string fileName, string contentType)
        {
            // Kiểm tra dữ liệu có hợp lệ không
            if (imageData == null || imageData.Length == 0)
                throw new ArgumentException("Image data is invalid");

            // Validate loại file (chỉ cho phép image)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(fileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException($"Unsupported image extension: {fileExtension}");
            }

            // Tạo MemoryStream từ byte array
            using var memoryStream = new MemoryStream(imageData);
            memoryStream.Position = 0;

            // Upload ảnh vào folder products trong Cloudinary
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, memoryStream),
                Folder = "products"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // Kiểm tra xem upload có thành công không
            if (uploadResult == null || uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var errorMsg = uploadResult?.Error?.Message ?? "Unknown error";
                var status = uploadResult?.StatusCode.ToString() ?? "Unknown";
                throw new Exception($"Cloudinary upload failed: {errorMsg} (Status: {status})");
            }

            return new CloudinaryUploadResult
            {
                Url = uploadResult.SecureUrl.ToString(),
                PublicId = uploadResult.PublicId
            };
        }

        public async Task DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new ArgumentException("PublicId is required");

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Result != "ok" && result.Result != "not found")
                throw new Exception($"Failed to delete image. Reason: {result.Result}");
        }
    }
}
