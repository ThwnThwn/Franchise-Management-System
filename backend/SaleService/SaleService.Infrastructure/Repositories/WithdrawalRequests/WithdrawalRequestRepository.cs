using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.ApplicationDbContext;

namespace SalesService.Infrastructure.Repositories.WithdrawalRequests;

public class WithdrawalRequestRepository : GenericRepository<WithdrawalRequest>, IWithdrawalRequestRepository
{
    public WithdrawalRequestRepository(ISalesDbContext dbContext) : base(dbContext)
    {
    }

    // =========================================================
    // NHÓM ĐỌC DỮ LIỆU (QUERIES)
    // =========================================================
    public async Task<PagedResult<WithdrawalRequest>> GetPagedWithdrawalsAsync(
        Guid? userId,
        WithdrawalStatus? status,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Where(x => !x.IsDeleted);

        // Build query linh hoạt, ai truyền gì thì filter nấy
        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        // Luôn sort mới nhất đưa lên đầu
        query = query.OrderByDescending(x => x.CreatedAt);

        return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
    }

    public async Task<WithdrawalRequest?> GetByRequestCodeAsync(string requestCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.RequestCode == requestCode && !x.IsDeleted, cancellationToken);
    }

    public async Task<bool> HasPendingWithdrawalAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.Status == WithdrawalStatus.Pending && !x.IsDeleted, cancellationToken);
    }

    // =========================================================
    // NHÓM LOGIC NỘI BỘ (Chỉ thay đổi state, KHÔNG SaveChanges)
    // Dùng cho Unit of Work hoặc gọi nội bộ
    // =========================================================

    public void ApplyApprove(WithdrawalRequest request, Guid approvedBy, string? approvalNotes)
    {
        request.Status = WithdrawalStatus.Approved;
        request.ApprovedBy = approvedBy;
        request.ApprovalNotes = approvalNotes;
        request.ApprovedAt = DateTimeOffset.UtcNow;
        request.UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ApplyReject(WithdrawalRequest request, Guid rejectedBy, string rejectionReason)
    {
        request.Status = WithdrawalStatus.Rejected;
        request.RejectedBy = rejectedBy;
        request.RejectionReason = rejectionReason;
        request.RejectedAt = DateTimeOffset.UtcNow;
        request.UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ApplyCancel(WithdrawalRequest request, Guid cancelledBy, string cancellationReason)
    {
        request.Status = WithdrawalStatus.Cancelled;
        request.CancelledBy = cancelledBy;
        request.CancellationReason = cancellationReason;
        request.CancelledAt = DateTimeOffset.UtcNow;
        request.UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ApplyComplete(WithdrawalRequest request, Guid processedBy, Guid transactionId, string? receiptImageUrl, string? receiptImagePublicId)
    {
        request.Status = WithdrawalStatus.Completed;
        request.ProcessedBy = processedBy;
        request.TransactionId = transactionId;
        request.ReceiptImageUrl = receiptImageUrl;
        request.ReceiptImagePublicId = receiptImagePublicId;
        request.CompletedAt = DateTimeOffset.UtcNow;
        request.UpdatedAt = DateTimeOffset.UtcNow;
    }

    // =========================================================
    // NHÓM PUBLIC COMMANDS (Gọi hàm cũ)
    // =========================================================

    public async Task<int> ApproveAsync(Guid id, Guid approvedBy, string? approvalNotes, CancellationToken cancellationToken = default)
    {
        var request = await _dbSet.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
        if (request == null) return 0;

        ApplyApprove(request, approvedBy, approvalNotes);
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RejectAsync(Guid id, Guid rejectedBy, string rejectionReason, CancellationToken cancellationToken = default)
    {
        var request = await _dbSet.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
        if (request == null) return 0;

        ApplyReject(request, rejectedBy, rejectionReason);
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CancelAsync(Guid id, Guid cancelledBy, string cancellationReason, CancellationToken cancellationToken = default)
    {
        var request = await _dbSet.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
        if (request == null) return 0;

        ApplyCancel(request, cancelledBy, cancellationReason);
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CompleteAsync(Guid id, Guid processedBy, Guid transactionId, string? receiptImageUrl, string? receiptImagePublicId, CancellationToken cancellationToken = default)
    {
        var request = await _dbSet.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
        if (request == null) return 0;

        ApplyComplete(request, processedBy, transactionId, receiptImageUrl, receiptImagePublicId);
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}