using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Features.ShiftAvailabilities.DTOs;
using MediatR;

namespace FranchiseService.Application.Features.ShiftAvailabilities.Queries.GetStaffAvailabilities;

// Query danh cho Manager, lay toan bo lich ranh/ban cua nhan vien
public class GetStaffAvailabilitiesQuery : PaginationRequest, IRequest<PagedResult<StaffAvailabilityResponse>>
{
    public Guid? StoreId { get; set; }
    public Guid? UserId { get; set; }

    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    
    public bool? IsAvailable { get; set; }
    public bool? IsRecurring { get; set; }
    public string? Keyword { get; set; }

    public GetStaffAvailabilitiesQuery() { }
}
