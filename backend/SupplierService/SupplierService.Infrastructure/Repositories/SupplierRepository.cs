using Microsoft.EntityFrameworkCore;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Infrastructure.DbContext;

namespace SupplierService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Supplier CRUD.
/// Sử dụng SupplierDbContext để truy cập DB.
/// </summary>
public class SupplierRepository : ISupplierRepository
{
    private readonly SupplierDbContext _dbContext;

    public SupplierRepository(SupplierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Lấy 1 supplier chưa bị xóa theo ID.</summary>
    public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted == false, cancellationToken);
    }

    /// <summary>Kiểm tra Code đã tồn tại chưa (loại trừ 1 ID nếu cần).</summary>
    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Suppliers
            .Where(s => s.Code == code && s.IsDeleted == false);

        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>Kiểm tra TaxCode đã tồn tại chưa (loại trừ 1 ID nếu cần).</summary>
    public async Task<bool> TaxCodeExistsAsync(string taxCode, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Suppliers
            .Where(s => s.TaxCode == taxCode && s.IsDeleted == false);

        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>Thêm supplier mới vào DB.</summary>
    public async Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        await _dbContext.Suppliers.AddAsync(supplier, cancellationToken);
    }

    /// <summary>Trả về IQueryable để query linh hoạt.</summary>
    public IQueryable<Supplier> GetQueryable()
    {
        return _dbContext.Suppliers.AsQueryable();
    }

    /// <summary>Lưu thay đổi vào DB.</summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
