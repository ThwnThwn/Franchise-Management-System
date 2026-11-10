using MediatR;

namespace SupplierService.Application.Features.Suppliers.Queries.GetSuppliers;

/// <summary>
/// Lấy danh sách nhà cung cấp có phân trang.
/// Hỗ trợ tìm kiếm theo tên/code và lọc theo trạng thái.
/// </summary>
public class GetSuppliersQuery : IRequest<GetSuppliersResult>
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetSuppliersResult
{
    public List<SupplierSummaryDto> Suppliers { get; set; } = new();
    public int TotalCount { get; set; }
}

public class SupplierSummaryDto
{
    public string Id { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ContactPerson { get; set; } = "";
    public string Phone { get; set; } = "";
    public bool IsActive { get; set; }
}
