using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Dtos.Cloudinary;
using IdentityService.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace IdentityService.Infrastructure.ExternalServices
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

        public async Task<CloudinaryUploadResult> UploadImageFromBytesAsync(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                throw new ArgumentException("Image data is invalid");

            if (imageData.Length > 5 * 1024 * 1024)
                throw new ArgumentException("Image size must not exceed 10MB");

            // Generate a unique file name for the image avoiding collisions in Cloudinary
            var fileName = $"{Guid.NewGuid().ToString()}";

            await using var stream = new MemoryStream(imageData);

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = "users/avatars"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult == null || uploadResult.StatusCode != HttpStatusCode.OK)
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
