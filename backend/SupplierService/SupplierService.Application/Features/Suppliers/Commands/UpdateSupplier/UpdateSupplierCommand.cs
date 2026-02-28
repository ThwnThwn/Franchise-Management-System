using MediatR;

namespace SupplierService.Application.Features.Suppliers.Commands.UpdateSupplier;

/// <summary>
/// Cập nhật thông tin nhà cung cấp (tên, liên hệ, địa chỉ, trạng thái...).
/// </summary>
public class UpdateSupplierCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public bool? IsActive { get; set; }
}
