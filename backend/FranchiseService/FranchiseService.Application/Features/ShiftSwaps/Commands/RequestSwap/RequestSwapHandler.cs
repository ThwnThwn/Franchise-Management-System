using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.ShiftSwaps.Helpers;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.ShiftSwaps.Commands.RequestSwap;

/// <summary>
/// Xu ly yeu cau doi ca tu nhan vien A.
///
/// Dung SwapValidationHelper cho cac check chung (assignment, Published, AllowShiftSwap).
/// Sau do kiem tra them: A khong pending, B hop le, B khong pending.
/// </summary>
public class RequestSwapHandler : IRequestHandler<RequestSwapCommand, RequestSwapResult>
{
    private readonly SwapValidationHelper _helper;
    private readonly IGenericRepository<ShiftSwapRequest> _swapRepo;
    private readonly IGenericRepository<ShiftAssignment> _assignmentRepo;
    private readonly IIdentityServiceClient _identityClient;

    public RequestSwapHandler(
        SwapValidationHelper helper,
        IGenericRepository<ShiftSwapRequest> swapRepo,
        IGenericRepository<ShiftAssignment> assignmentRepo,
        IIdentityServiceClient identityClient)
    {
        _helper = helper;
        _swapRepo = swapRepo;
        _assignmentRepo = assignmentRepo;
        _identityClient = identityClient;
    }

    public async Task<RequestSwapResult> Handle(
        RequestSwapCommand request, CancellationToken ct)
    {
        // 1. Kiem tra dieu kien chung (assignment, Published, AllowShiftSwap)
        var check = await _helper.ValidatePreconditionsAsync(
            request.AssignmentId, request.SourceUserId);

        // 2. A khong co swap pending nao khac
        var aPending = await _swapRepo.GetAllAsync(
            predicate: s => s.SourceUserId == request.SourceUserId
                && (s.Status == (int)SwapRequestStatus.PendingTarget
                    || s.Status == (int)SwapRequestStatus.PendingManager));
        if (aPending.Any())
            throw new InvalidOperationException(
                "Ban dang co yeu cau doi ca cho xu ly.");

        // 3. B thuoc cung Store
        var targetUser = await _identityClient.GetUserByIdAsync(request.TargetUserId);
        if (targetUser == null || targetUser.HomeStoreId != check.Shift.StoreId)
            throw new InvalidOperationException(
                "Nguoi nhan khong thuoc cung cua hang.");

        // 4. B chua duoc gan cung ca nay
        var targetInShift = await _assignmentRepo.GetAllAsync(
            predicate: a => a.ShiftId == check.Shift.Id
                && a.UserId == request.TargetUserId);
        if (targetInShift.Any())
            throw new InvalidOperationException(
                "Nguoi nhan da duoc gan ca nay roi.");

        // 5. B khong co swap pending (tranh deadlock)
        var bPending = await _swapRepo.GetAllAsync(
            predicate: s => (s.SourceUserId == request.TargetUserId
                          || s.TargetUserId == request.TargetUserId)
                && (s.Status == (int)SwapRequestStatus.PendingTarget
                    || s.Status == (int)SwapRequestStatus.PendingManager));
        if (bPending.Any())
            throw new InvalidOperationException(
                "Nguoi nhan dang co yeu cau doi ca khac cho xu ly.");

        // 6. Tao yeu cau
        var swapRequest = new ShiftSwapRequest
        {
            Id = Guid.NewGuid(),
            SourceAssignmentId = request.AssignmentId,
            SourceUserId = request.SourceUserId,
            TargetUserId = request.TargetUserId,
            Status = (int)SwapRequestStatus.PendingTarget,
            Reason = request.Reason,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        await _swapRepo.AddAsync(swapRequest);

        return new RequestSwapResult
        {
            SwapRequestId = swapRequest.Id,
            ExpiresAt = swapRequest.ExpiresAt
        };
    }
}
