using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Features.ShiftAvailabilities.DTOs;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;
using System.Linq.Expressions;

namespace FranchiseService.Application.Features.ShiftAvailabilities.Queries.GetMyAvailability;

public class GetMyAvailabilityHandler : IRequestHandler<GetMyAvailabilityQuery, PagedResult<StaffAvailabilityResponse>>
{
    private readonly IGenericRepository<StaffAvailability> _repository;

    public GetMyAvailabilityHandler(IGenericRepository<StaffAvailability> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<StaffAvailabilityResponse>> Handle(GetMyAvailabilityQuery request, CancellationToken cancellationToken)
    {
        // 1. Fetch ALL records for this user from DB
        // Predicate don gian de tranh loi Expression duplicate
        var allRecords = await _repository.GetAllAsync(predicate: x => x.UserId == request.UserId);

        // 2. Apply Filters in Memory
        var query = allRecords.AsQueryable();

        // Filter: IsAvailable
        if (request.IsAvailable.HasValue)
        {
            query = query.Where(x => x.IsAvailable == request.IsAvailable.Value);
        }

        // Filter: IsRecurring
        if (request.IsRecurring.HasValue)
        {
            query = query.Where(x => x.IsRecurring == request.IsRecurring.Value);
        }

        // Filter: DayOfWeek
        if (request.DayOfWeek.HasValue)
        {
            query = query.Where(x => x.DayOfWeek == (short)request.DayOfWeek.Value);
        }

        // Filter: Keyword (Note)
        if (!string.IsNullOrEmpty(request.Keyword))
        {
            var k = request.Keyword.ToLower();
            query = query.Where(x => x.Note != null && x.Note.ToLower().Contains(k));
        }

        // Filter: Date Range
        // Logic: Keep if Recurring OR (SpecificDate within Range)
        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.IsRecurring == true || (x.SpecificDate >= request.FromDate));
        }
        if (request.ToDate.HasValue)
        {
            query = query.Where(x => x.IsRecurring == true || (x.SpecificDate <= request.ToDate));
        }

        // 3. Pagination
        var filteredList = query.OrderByDescending(x => x.CreatedAt).ToList();
        var totalCount = filteredList.Count;
        var pagedItems = filteredList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var items = pagedItems.Select(x => new StaffAvailabilityResponse
        {
            Id = x.Id,
            UserId = x.UserId,
            IsAvailable = x.IsAvailable,
            IsRecurring = x.IsRecurring ?? false,
            SpecificDate = x.SpecificDate,
            DayOfWeek = x.DayOfWeek.HasValue ? (DayOfWeek)x.DayOfWeek.Value : null,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            Note = x.Note,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        }).ToList();

        return new PagedResult<StaffAvailabilityResponse>(items, totalCount, request.Page, request.PageSize);
    }
}
