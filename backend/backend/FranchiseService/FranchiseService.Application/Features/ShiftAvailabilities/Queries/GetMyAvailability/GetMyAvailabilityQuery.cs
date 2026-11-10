using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Features.ShiftAvailabilities.DTOs;
using MediatR;

namespace FranchiseService.Application.Features.ShiftAvailabilities.Queries.GetMyAvailability;

public class GetMyAvailabilityQuery : PaginationRequest, IRequest<PagedResult<StaffAvailabilityResponse>>
{
    public Guid UserId { get; set; } // Tu x-user-id

    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    
    public bool? IsAvailable { get; set; }
    public bool? IsRecurring { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public string? Keyword { get; set; }

    public GetMyAvailabilityQuery() { }
}
