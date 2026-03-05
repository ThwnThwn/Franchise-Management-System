using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;
using System.Linq.Expressions;

namespace FranchiseService.Application.Features.ShiftSwaps.Queries.GetMySwapRequests;

/// <summary>
/// Lay danh sach yeu cau doi ca lien quan den toi.
/// Tu dong danh dau Expired cho yeu cau qua han (Lazy check).
/// </summary>
public class GetMySwapRequestsHandler
    : IRequestHandler<GetMySwapRequestsQuery, List<SwapRequestDto>>
{
    private readonly IGenericRepository<ShiftSwapRequest> _swapRepo;
    private readonly IIdentityServiceClient _identityClient;

    public GetMySwapRequestsHandler(
        IGenericRepository<ShiftSwapRequest> swapRepo,
        IIdentityServiceClient identityClient)
    {
        _swapRepo = swapRepo;
        _identityClient = identityClient;
    }

    public async Task<List<SwapRequestDto>> Handle(
        GetMySwapRequestsQuery request, CancellationToken ct)
    {
        // Lay tat ca yeu cau lien quan den user (gui di va nhan duoc)
        var swaps = (await _swapRepo.GetAllAsync(
            includes: new Expression<Func<ShiftSwapRequest, object>>[]
            {
                s => s.SourceAssignment,
                s => s.SourceAssignment.Shift
            },
            predicate: s => s.SourceUserId == request.UserId
                         || s.TargetUserId == request.UserId
        )).ToList();

        // Lazy check: danh dau Expired cho yeu cau qua han
        foreach (var swap in swaps.Where(s =>
            s.Status == (int)SwapRequestStatus.PendingTarget
            && DateTimeOffset.UtcNow > s.ExpiresAt))
        {
            swap.Status = (int)SwapRequestStatus.Expired;
            swap.UpdatedAt = DateTimeOffset.UtcNow;
            await _swapRepo.UpdateAsync(swap);
        }

        // Batch fetch ten nhan vien (1 query duy nhat)
        var userIds = swaps
            .SelectMany(s => new[] { s.SourceUserId, s.TargetUserId })
            .Distinct().ToList();
        var users = await _identityClient.GetUsersByIdsAsync(userIds);
        var userMap = users.ToDictionary(u => u.Id);

        // Map thanh DTO
        return swaps.Select(s =>
        {
            userMap.TryGetValue(s.SourceUserId, out var src);
            userMap.TryGetValue(s.TargetUserId, out var tgt);
            var shift = s.SourceAssignment?.Shift;

            return new SwapRequestDto
            {
                Id = s.Id,
                SourceUserId = s.SourceUserId,
                SourceUserName = src != null
                    ? $"{src.FirstName} {src.LastName}".Trim() : null,
                TargetUserId = s.TargetUserId,
                TargetUserName = tgt != null
                    ? $"{tgt.FirstName} {tgt.LastName}".Trim() : null,
                Status = s.Status,
                Reason = s.Reason,
                RejectReason = s.RejectReason,
                ExpiresAt = s.ExpiresAt,
                CreatedAt = s.CreatedAt,
                ShiftId = shift?.Id ?? Guid.Empty,
                ShiftDate = shift?.ShiftDate ?? default,
                StartTime = shift?.StartTime ?? default,
                EndTime = shift?.EndTime ?? default
            };
        }).OrderByDescending(d => d.CreatedAt).ToList();
    }
}
