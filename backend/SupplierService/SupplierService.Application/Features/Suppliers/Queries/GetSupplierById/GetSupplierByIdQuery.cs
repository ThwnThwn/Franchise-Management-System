using MediatR;
using SupplierService.Domain.Entities;

namespace SupplierService.Application.Features.Suppliers.Queries.GetSupplierById;

/// <summary>
/// Lấy chi tiết 1 nhà cung cấp theo ID.
/// Trả về toàn bộ thông tin liên hệ + trạng thái.
/// </summary>
public record GetSupplierByIdQuery(string Id) : IRequest<SupplierDetailDto>;

public class SupplierDetailDto
{
    public string Id { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ContactPerson { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public string TaxCode { get; set; } = "";
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
}
