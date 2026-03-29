using MediatR;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;

namespace SupplierService.Application.Features.Suppliers.Commands.CreateSupplier;

/// <summary>
/// Handler tạo mới nhà cung cấp.
/// Kiểm tra trùng Code trước khi tạo.
/// </summary>
public class CreateSupplierHandler(
    ISupplierRepository repo
) : IRequestHandler<CreateSupplierCommand, CreateSupplierResult>
{
    public async Task<CreateSupplierResult> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra trùng TaxCode
        if (!string.IsNullOrWhiteSpace(request.TaxCode) && await repo.TaxCodeExistsAsync(request.TaxCode, cancellationToken: cancellationToken))
            throw new InvalidOperationException($"Supplier tax code '{request.TaxCode}' already exists.");

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            Code = $"NCC-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            TaxCode = request.TaxCode,
            IsActive = true
        };

        await repo.AddAsync(supplier, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);

        return new CreateSupplierResult
        {
            Id = supplier.Id,
            Code = supplier.Code
        };
    }
}