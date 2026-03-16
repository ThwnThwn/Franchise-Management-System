using FranchiseService.Domain.Commons;
using System;
using System.Collections.Generic;

namespace FranchiseService.Domain.Models;

public partial class FranchiseApplication : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public Guid ManagerId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string IdentityNumber { get; set; } = null!;

    public string StoreAddress { get; set; } = null!;

    public decimal? StoreLatitude { get; set; }

    public decimal? StoreLongitude { get; set; }

    public decimal? StoreSizeSqm { get; set; }

    public string? LandCertificateUrl { get; set; }

    public string? LandCertificatePublicId { get; set; }

    public string? StoreFrontUrl { get; set; }

    public string? StoreFrontPublicId { get; set; }

    public int? Status { get; set; }

    public string? AdminNote { get; set; }

    public int? DocusealSubmissionId { get; set; }

    public string? ContractPdfUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public Guid? ReviewedBy { get; set; }
}
