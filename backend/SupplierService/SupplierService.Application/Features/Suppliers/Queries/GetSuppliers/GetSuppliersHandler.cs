using MediatR;
using Microsoft.EntityFrameworkCore;
using SupplierService.Application.Repositories;

namespace SupplierService.Application.Features.Suppliers.Queries.GetSuppliers;

/// <summary>
/// Handler lấy danh sách nhà cung cấp có phân trang.
/// Hỗ trợ tìm theo tên/code (search) và lọc theo trạng thái (is_active).
/// </summary>
public class GetSuppliersHandler : IRequestHandler<GetSuppliersQuery, GetSuppliersResult>
{
    private readonly ISupplierRepository _repo;

    public GetSuppliersHandler(ISupplierRepository repo)
    {
        _repo = repo;
    }

    public async Task<GetSuppliersResult> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        var query = _repo.GetQueryable()
            .Where(s => s.IsDeleted == false);

        // Tìm kiếm theo code
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var codeSearch = request.Code.ToLower();
            query = query.Where(s => s.Code.ToLower().Contains(codeSearch));
        }

        // Tìm kiếm theo tên
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var nameSearch = request.Name.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(nameSearch));
        }

        // Lọc theo trạng thái
        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var suppliers = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SupplierSummaryDto
            {
                Id = s.Id.ToString(),
                Code = s.Code,
                Name = s.Name,
                ContactPerson = s.ContactPerson ?? "",
                Phone = s.Phone ?? "",
                IsActive = s.IsActive
            })
            .ToListAsync(cancellationToken);

        return new GetSuppliersResult
        {
            Suppliers = suppliers,
            TotalCount = totalCount
        };
    }
}
