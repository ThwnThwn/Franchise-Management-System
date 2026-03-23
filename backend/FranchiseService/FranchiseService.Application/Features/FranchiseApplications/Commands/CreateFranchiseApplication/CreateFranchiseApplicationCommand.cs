using MediatR;

namespace FranchiseService.Application.Features.FranchiseApplications.Commands.CreateFranchiseApplication;

/// <summary>
/// Command: Đối tác gửi hồ sơ đăng ký nhượng quyền.
/// Dùng bytes[] để tương thích với gRPC (không dùng IFormFile).
/// </summary>
public class CreateFranchiseApplicationCommand : IRequest<Guid>
{
    public Guid StoreId { get; set; }
    public Guid ManagerId { get; set; }

    // Thông tin cá nhân
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string IdentityNumber { get; set; } = null!;

    // Địa chỉ phân cấp
    public string? ProvinceId { get; set; }
    public string? DistrictId { get; set; }
    public string? WardId { get; set; }
    public string StoreAddress { get; set; } = null!;
    public decimal? StoreLatitude { get; set; }
    public decimal? StoreLongitude { get; set; }
    public decimal? StoreSizeSqm { get; set; }

    /// <summary>
    /// Danh sách file chứng nhận đất đai (nhiều file).
    /// Mỗi phần tử là (Data: bytes, FileName: tên file, ContentType: MIME type).
    /// </summary>
    public List<FileUploadItem>? LandCertificateFiles { get; set; }

    public FileUploadItem? StoreFrontFile { get; set; }
}

public class FileUploadItem
{
    public byte[] Data { get; set; } = [];
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/jpeg";
}
