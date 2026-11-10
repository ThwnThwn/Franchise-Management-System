using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.ShiftSwaps.Queries.GetMySwapRequests;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;
using System.Linq.Expressions;

namespace FranchiseService.Application.Features.ShiftSwaps.Queries.GetSwapRequestsByStore;

/// <summary>
/// Xử lý logic truy vấn danh sách yêu cầu đổi ca theo Store (cửa hàng).
/// Sử dụng EF Core Includes để lấy thông tin assignment và shift nhanh chóng (tránh N+1 query).
/// Kết hợp lấy thông tin user identity.
/// </summary>
public class GetSwapRequestsByStoreHandler
    : IRequestHandler<GetSwapRequestsByStoreQuery, List<SwapRequestDto>>
{
    private readonly IGenericRepository<ShiftSwapRequest> _swapRepo;
    private readonly IIdentityServiceClient _identityClient;

    public GetSwapRequestsByStoreHandler(
        IGenericRepository<ShiftSwapRequest> swapRepo,
        IIdentityServiceClient identityClient)
    {
        _swapRepo = swapRepo;
        _identityClient = identityClient;
    }

    public async Task<List<SwapRequestDto>> Handle(
        GetSwapRequestsByStoreQuery request, CancellationToken ct)
    {
        // Lấy danh sách yêu cầu đổi ca thuộc các ca làm việc chung của Store (kèm lọc status nếu có)
        var swaps = (await _swapRepo.GetAllAsync(
            includes: new Expression<Func<ShiftSwapRequest, object>>[]
            {
                // Include thông tin assignment rồi mới đến shift của assignment đó
                s => s.SourceAssignment,
                s => s.SourceAssignment.Shift
            },
            predicate: s => s.SourceAssignment.Shift.StoreId == request.StoreId
                         && (!request.Status.HasValue || s.Status == request.Status.Value)
        )).ToList();

        // Kiểm tra loại bỏ những yêu cầu quá hạn đang chờ (Lazy check)
        bool hasExpired = false;
        foreach (var swap in swaps.Where(s =>
            s.Status == (int)SwapRequestStatus.PendingTarget
            && DateTimeOffset.UtcNow > s.ExpiresAt))
        {
            swap.Status = (int)SwapRequestStatus.Expired;
            swap.UpdatedAt = DateTimeOffset.UtcNow;
            await _swapRepo.UpdateAsync(swap);
            hasExpired = true;
        }

        // Kiểm tra bộ lọc hiện tại: nếu Frontend đang xin lấy đơn Pending nhưng nó mới bị update thành Expired,
        // thì phải lọc tiếp qua memory danh sách đó để khỏi bị nhầm lẫn trên màn hình.
        if (request.Status.HasValue && hasExpired)
        {
            swaps = swaps.Where(s => s.Status == request.Status.Value).ToList();
        }

        // Tối ưu hóa gọi Identity API 1 lần duy nhất cho toàn bộ danh sách người gửi/nhận đơn
        var userIds = swaps
            .SelectMany(s => new[] { s.SourceUserId, s.TargetUserId })
            .Distinct().ToList();
        
        var users = await _identityClient.GetUsersByIdsAsync(userIds);
        var userMap = users.ToDictionary(u => u.Id);

        // Transform data đưa ra DTO.
        return swaps.Select(s =>
        {
            userMap.TryGetValue(s.SourceUserId, out var src);
            userMap.TryGetValue(s.TargetUserId, out var tgt);
            var shift = s.SourceAssignment?.Shift;

            return new SwapRequestDto
            {
                Id = s.Id,
                SourceUserId = s.SourceUserId,
                SourceUserName = src != null ? $"{src.FirstName} {src.LastName}".Trim() : null,
                TargetUserId = s.TargetUserId,
                TargetUserName = tgt != null ? $"{tgt.FirstName} {tgt.LastName}".Trim() : null,
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
