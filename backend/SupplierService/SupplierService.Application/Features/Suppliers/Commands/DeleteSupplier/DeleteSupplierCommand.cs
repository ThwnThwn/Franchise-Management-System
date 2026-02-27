using MediatR;

namespace SupplierService.Application.Features.Suppliers.Commands.DeleteSupplier;

/// <summary>
/// Xóa mềm nhà cung cấp (set IsDeleted = true).
/// Không xóa vật lý để giữ lịch sử PO liên quan.
/// </summary>
public record DeleteSupplierCommand(Guid Id) : IRequest<Unit>;
