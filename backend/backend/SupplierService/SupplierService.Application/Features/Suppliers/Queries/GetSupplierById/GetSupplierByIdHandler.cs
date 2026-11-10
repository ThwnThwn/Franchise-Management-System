using MediatR;
using SupplierService.Application.Repositories;

namespace SupplierService.Application.Features.Suppliers.Queries.GetSupplierById;

/// <summary>
/// Handler lấy chi tiết 1 nhà cung cấp theo ID.
/// Chỉ trả về supplier chưa bị xóa (IsDeleted = false).
/// </summary>
public class GetSupplierByIdHandler(
    ISupplierRepository repo
) : IRequestHandler<GetSupplierByIdQuery, SupplierDetailDto>
{
    public async Task<SupplierDetailDto> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new ArgumentException("Invalid Supplier ID.");

        var supplier = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Supplier {request.Id} not found.");

        return new SupplierDetailDto
        {
            Id = supplier.Id.ToString(),
            Code = supplier.Code,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson ?? "",
            Phone = supplier.Phone ?? "",
            Email = supplier.Email ?? "",
            Address = supplier.Address ?? "",
            TaxCode = supplier.TaxCode ?? "",
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt.ToString("o"),
            UpdatedAt = supplier.UpdatedAt.ToString("o")
        };
    }
}