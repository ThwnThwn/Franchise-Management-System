using MediatR;

namespace SupplierService.Application.Features.Suppliers.Commands.CreateSupplier;

/// <summary>
/// Tạo mới nhà cung cấp. Trả về ID và Code sau khi tạo thành công.
/// </summary>
public class CreateSupplierCommand : IRequest<CreateSupplierResult>
{
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
}

public class CreateSupplierResult
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
}
