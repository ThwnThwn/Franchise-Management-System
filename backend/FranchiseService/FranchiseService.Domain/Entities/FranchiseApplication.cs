using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Domain.Models;

public partial class FranchiseApplication : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public Guid ManagerId { get; set; }

    // --------------- Thông tin cá nhân đối tác ---------------

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string IdentityNumber { get; set; } = null!;

    // --------------- Thông tin mặt bằng & Địa chỉ ---------------

    public string? ProvinceId { get; set; }

    public string? DistrictId { get; set; }

    public string? WardId { get; set; }

    public string StoreName { get; set; } = null!;

    public string StoreAddress { get; set; } = null!;

    public decimal? StoreLatitude { get; set; }

    public decimal? StoreLongitude { get; set; }

    public decimal? StoreSizeSqm { get; set; }

    // --------------- Tài liệu ---------------

    /// <summary>
    /// Danh sách link ảnh chứng nhận đất đai (sổ đỏ/hồng, hợp đồng thuê nhà...).
    /// Map với cột TEXT[] của PostgreSQL để hỗ trợ nhiều giấy tờ.
    /// </summary>
    public string[]? LandCertificates { get; set; }

    public string? StoreFrontUrl { get; set; }

    public string? StoreFrontPublicId { get; set; }

    // --------------- Trạng thái phê duyệt ---------------

    public FranchiseApplicationStatus Status { get; set; } = FranchiseApplicationStatus.Pending;

    public string? AdminNote { get; set; }

    // --------------- Audit ---------------

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public Guid? ReviewedBy { get; set; }

    // --------------- Navigation Properties ---------------

    public virtual ICollection<FranchiseContract> Contracts { get; set; } = new List<FranchiseContract>();
}