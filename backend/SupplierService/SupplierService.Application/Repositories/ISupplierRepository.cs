using SupplierService.Domain.Entities;

namespace SupplierService.Application.Repositories;

/// <summary>
/// Repository interface cho Supplier CRUD.
/// Abstraction layer giữa Application và Infrastructure, tuân theo Clean Architecture.
/// </summary>
public interface ISupplierRepository
{
    /// <summary>Lấy 1 supplier chưa bị xóa theo ID.</summary>
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Kiểm tra Code đã tồn tại chưa (loại trừ 1 ID nếu cần).</summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>Kiểm tra TaxCode đã tồn tại chưa (loại trừ 1 ID nếu cần).</summary>
    Task<bool> TaxCodeExistsAsync(string taxCode, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>Thêm supplier mới vào DB.</summary>
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);

    /// <summary>Trả về IQueryable để query linh hoạt (phân trang, tìm kiếm).</summary>
    IQueryable<Supplier> GetQueryable();

    /// <summary>Lưu thay đổi vào DB.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
