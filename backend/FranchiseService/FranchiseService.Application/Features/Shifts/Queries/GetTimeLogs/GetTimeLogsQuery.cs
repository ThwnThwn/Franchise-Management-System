using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Features.Shifts.DTOs;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetTimeLogs;

/// <summary>
/// Query lấy danh sách bản ghi chấm công có thể phân trang.
/// Manager sẽ lọc cả Store, Staff chỉ lọc của chính họ.
/// </summary>
public class GetTimeLogsQuery : IRequest<PagedResult<TimeLogDto>>
{
    public Guid? StoreId { get; set; }    // Lọc theo cửa hàng (Dùng cho Manager)
    public Guid? StaffId { get; set; }    // Lọc theo tài khoản nhân viên
    public int? Status { get; set; }      // Trạng thái TimeLog
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
