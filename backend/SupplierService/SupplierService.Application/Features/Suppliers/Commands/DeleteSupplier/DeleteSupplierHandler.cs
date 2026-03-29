using MediatR;
using SupplierService.Application.Repositories;

namespace SupplierService.Application.Features.Suppliers.Commands.DeleteSupplier;

/// <summary>
/// Handler xóa mềm nhà cung cấp.
/// Set IsDeleted = true, không xóa vật lý khỏi DB để giữ lịch sử PO.
/// </summary>
public class DeleteSupplierHandler(
    ISupplierRepository repo
) : IRequestHandler<DeleteSupplierCommand, Unit>
{
    public async Task<Unit> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await repo.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Supplier {request.Id} not found.");

        supplier.IsDeleted = true;
        await repo.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}