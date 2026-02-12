using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.ShiftAvailabilities.DTOs;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;
using System.Linq.Expressions;

namespace FranchiseService.Application.Features.ShiftAvailabilities.Queries.GetStaffAvailabilities;

public class GetStaffAvailabilitiesHandler : IRequestHandler<GetStaffAvailabilitiesQuery, PagedResult<StaffAvailabilityResponse>>
{
    private readonly IGenericRepository<StaffAvailability> _repository;
    private readonly IIdentityServiceClient _identityClient;

    public GetStaffAvailabilitiesHandler(
        IGenericRepository<StaffAvailability> repository,
        IIdentityServiceClient identityClient)
    {
        _repository = repository;
        _identityClient = identityClient;
    }

    public async Task<PagedResult<StaffAvailabilityResponse>> Handle(GetStaffAvailabilitiesQuery request, CancellationToken cancellationToken)
    {
        // 1. Identify Target Users
        List<Guid>? targetUserIds = null;

        if (request.StoreId.HasValue)
        {
            var allStaffs = await _identityClient.GetAllUsersAsync("staff");
            targetUserIds = allStaffs
                .Where(u => u.HomeStoreId == request.StoreId.Value)
                .Select(u => u.Id)
                .ToList();

            if (!targetUserIds.Any())
            {
                return new PagedResult<StaffAvailabilityResponse>(new List<StaffAvailabilityResponse>(), 0, request.Page, request.PageSize);
            }
        }

        if (request.UserId.HasValue)
        {
            if (targetUserIds != null)
            {
                if (!targetUserIds.Contains(request.UserId.Value))
                {
                    return new PagedResult<StaffAvailabilityResponse>(new List<StaffAvailabilityResponse>(), 0, request.Page, request.PageSize);
                }
                targetUserIds = new List<Guid> { request.UserId.Value };
            }
            else
            {
                targetUserIds = new List<Guid> { request.UserId.Value };
            }
        }

        // 2. Fetch from DB
        IEnumerable<StaffAvailability> allRecords;
        
        if (targetUserIds != null)
        {
            // Predicate: UserId IN targetUserIds
            allRecords = await _repository.GetAllAsync(predicate: x => targetUserIds.Contains(x.UserId));
        }
        else
        {
            // No user filter -> Fetch ALL (Warning: Large dataset potential)
            // But if generic GetAllAsync is efficient enough or dataset is small
            allRecords = await _repository.GetAllAsync();
        }

        // 3. Filter in Memory
        var query = allRecords.AsQueryable();

        if (request.IsAvailable.HasValue)
            query = query.Where(x => x.IsAvailable == request.IsAvailable.Value);

        if (request.IsRecurring.HasValue)
            query = query.Where(x => x.IsRecurring == request.IsRecurring.Value);

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            var k = request.Keyword.ToLower();
            query = query.Where(x => x.Note != null && x.Note.ToLower().Contains(k));
        }

        if (request.FromDate.HasValue)
            query = query.Where(x => x.IsRecurring == true || (x.SpecificDate >= request.FromDate));

        if (request.ToDate.HasValue)
            query = query.Where(x => x.IsRecurring == true || (x.SpecificDate <= request.ToDate));

        // 4. Pagination
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
