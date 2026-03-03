using MediatR;
using SupplierService.Application.Repositories;

namespace SupplierService.Application.Features.Suppliers.Commands.DeleteSupplier;

/// <summary>
/// Handler xóa mềm nhà cung cấp.
/// Set IsDeleted = true, không xóa vật lý khỏi DB để giữ lịch sử PO.
/// </summary>
public class DeleteSupplierHandler : IRequestHandler<DeleteSupplierCommand, Unit>
{
    private readonly ISupplierRepository _repo;

    public DeleteSupplierHandler(ISupplierRepository repo)
    {
        _repo = repo;
    }

    public async Task<Unit> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repo.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Supplier {request.Id} not found.");

        supplier.IsDeleted = true;
        await _repo.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
