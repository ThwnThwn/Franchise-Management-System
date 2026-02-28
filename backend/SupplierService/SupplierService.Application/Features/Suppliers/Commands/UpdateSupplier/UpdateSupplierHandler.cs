using MediatR;
using SupplierService.Application.Repositories;

namespace SupplierService.Application.Features.Suppliers.Commands.UpdateSupplier;

/// <summary>
/// Handler cập nhật thông tin nhà cung cấp.
/// Kiểm tra tồn tại + trùng Code (nếu đổi Code) trước khi update.
/// </summary>
public class UpdateSupplierHandler : IRequestHandler<UpdateSupplierCommand, Unit>
{
    private readonly ISupplierRepository _repo;

    public UpdateSupplierHandler(ISupplierRepository repo)
    {
        _repo = repo;
    }

    public async Task<Unit> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repo.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Supplier {request.Id} not found.");

        if (request.TaxCode != null)
        {
            // Kiểm tra trùng TaxCode (nếu đổi TaxCode)
            if (!string.IsNullOrWhiteSpace(request.TaxCode) && supplier.TaxCode != request.TaxCode)
            {
                if (await _repo.TaxCodeExistsAsync(request.TaxCode, request.Id, cancellationToken))
                    throw new InvalidOperationException($"Supplier tax code '{request.TaxCode}' already exists.");
            }
            supplier.TaxCode = request.TaxCode;
        }

        if (request.Name != null) supplier.Name = request.Name;
        if (request.ContactPerson != null) supplier.ContactPerson = request.ContactPerson;
        if (request.Phone != null) supplier.Phone = request.Phone;
        if (request.Email != null) supplier.Email = request.Email;
        if (request.Address != null) supplier.Address = request.Address;
        if (request.IsActive.HasValue) supplier.IsActive = request.IsActive.Value;

        await _repo.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
