using MediatR;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;

namespace SupplierService.Application.Features.Suppliers.Commands.CreateSupplier;

/// <summary>
/// Handler tạo mới nhà cung cấp.
/// Kiểm tra trùng Code trước khi tạo.
/// </summary>
public class CreateSupplierHandler : IRequestHandler<CreateSupplierCommand, CreateSupplierResult>
{
    private readonly ISupplierRepository _repo;

    public CreateSupplierHandler(ISupplierRepository repo)
    {
        _repo = repo;
    }

    public async Task<CreateSupplierResult> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra trùng TaxCode
        if (!string.IsNullOrWhiteSpace(request.TaxCode) && await _repo.TaxCodeExistsAsync(request.TaxCode, cancellationToken: cancellationToken))
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

        await _repo.AddAsync(supplier, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return new CreateSupplierResult
        {
            Id = supplier.Id,
            Code = supplier.Code
        };
    }
}
