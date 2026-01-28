using System;
using System.Collections.Generic;
using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities;

public partial class Franchisee : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? TaxCode { get; set; }

    public string? RepresentativeName { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? BankName { get; set; }

    public string? BankAccountNumber { get; set; }

    public string? BankAccountHolder { get; set; }

    public DateTime? ContractStartDate { get; set; }

    public DateTime? ContractEndDate { get; set; }

    public decimal? CommissionRate { get; set; }

    public bool? IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
}