using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SalesService.Application.Dtos;
using SalesService.Application.Interfaces;
using SalesService.Infrastructure.Settings;

namespace SalesService.Infrastructure.Services
{
    public class CloudinaryService : IImageService
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

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
        {
            // 1. Kiểm tra sơ bộ
            if (file == null || file.Length == 0)
                throw new ArgumentException("File upload is invalid");

            // 2. Chuẩn hóa Extension (Bất chấp FileName có "ngáo" hay không)
            var fileExtension = Path.GetExtension(file.FileName)?.ToLower() ?? "";

            // Nếu Client gửi blob/file không đuôi, cứu bằng ContentType
            if (string.IsNullOrEmpty(fileExtension))
            {
                fileExtension = file.ContentType.ToLower() switch
                {
                    "image/jpeg" or "image/jpg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    _ => ""
                };
            }

            // 3. Validate gắt gao
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException($"Unsupported image extension: {fileExtension} (Type: {file.ContentType})");
            }

            // 4. Tối ưu Stream (Dùng thẳng Stream của IFormFile để đỡ tốn RAM copy)
            using var stream = file.OpenReadStream();

            // Đảm bảo có tên file cho Cloudinary, nếu không có thì tự chế
            var finalFileName = string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(file.FileName))
                ? $"upload_{Guid.NewGuid()}{fileExtension}"
                : file.FileName;

            // 5. Cấu hình Upload
            var uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams
            {
                File = new FileDescription(finalFileName, stream),
                Folder = "sales",
                // có thể thêm option tự động tối ưu ảnh của Cloudinary ở đây
                Transformation = new CloudinaryDotNet.Transformation().Quality("auto").FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // 6. Check kết quả
            if (uploadResult == null || uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var errorMsg = uploadResult?.Error?.Message ?? "Unknown error";
                throw new Exception($"Cloudinary upload failed: {errorMsg}");
            }

            return new ImageUploadResult
            {
                Url = uploadResult.SecureUrl.ToString(),
                PublicId = uploadResult.PublicId
            };
        }

        public async Task<ImageUploadResult> UploadImageFromBytesAsync(byte[] imageData, string fileName, string contentType)
        {
            if (imageData == null || imageData.Length == 0)
                throw new ArgumentException("Image data is invalid");

            // 1. Lấy extension từ fileName (để lowercase)
            var fileExtension = Path.GetExtension(fileName)?.ToLower() ?? "";

            // 2. TUYỆT CHIÊU: Nếu fileName không có đuôi, hãy nhìn vào contentType
            if (string.IsNullOrEmpty(fileExtension))
            {
                fileExtension = contentType.ToLower() switch
                {
                    "image/jpeg" => ".jpg",
                    "image/jpg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    _ => ""
                };
            }

            // 3. Kiểm tra lại lần nữa
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException($"Unsupported image extension: {fileExtension} (From file: {fileName})");
            }

            // 4. Nếu fileName bị trống (gRPC hay bị), tự chế một cái tên để Cloudinary nó chịu nhận
            if (string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(fileName)))
            {
                fileName = $"upload_{Guid.NewGuid()}{fileExtension}";
            }

            using var memoryStream = new MemoryStream(imageData);
            memoryStream.Position = 0;

            var uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams
            {
                File = new FileDescription(fileName, memoryStream),
                Folder = "sales"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // Kiểm tra xem upload có thành công không
            if (uploadResult == null || uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var errorMsg = uploadResult?.Error?.Message ?? "Unknown error";
                var status = uploadResult?.StatusCode.ToString() ?? "Unknown";
                throw new Exception($"Cloudinary upload failed: {errorMsg} (Status: {status})");
            }

            return new ImageUploadResult
            {
                Url = uploadResult.SecureUrl.ToString(),
                PublicId = uploadResult.PublicId
            };
        }

        public async Task DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new ArgumentException("PublicId is required");

            var deletionParams = new CloudinaryDotNet.Actions.DeletionParams(publicId)
            {
                ResourceType = CloudinaryDotNet.Actions.ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Result != "ok" && result.Result != "not found")
                throw new Exception($"Failed to delete image. Reason: {result.Result}");
        }
    }
}