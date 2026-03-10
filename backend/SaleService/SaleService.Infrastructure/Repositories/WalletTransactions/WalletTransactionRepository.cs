using Microsoft.EntityFrameworkCore;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.ApplicationDbContext;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;

namespace SalesService.Infrastructure.Repositories;

public sealed class WalletTransactionRepository
    : GenericRepository<WalletTransaction>, IWalletTransactionRepository
{
    private readonly ISalesDbContext _context;

    public WalletTransactionRepository(ISalesDbContext dbContext) : base(dbContext)
    {
        _context = dbContext;
    }

    public async Task<IEnumerable<WalletTransaction>> GetAllByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.WalletId == walletId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<WalletTransaction?> GetByExternalTransactionIdAsync(
    string externalTransactionId,
    CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.ExternalTransactionId == externalTransactionId && !t.IsDeleted,
                cancellationToken);
    }

    public async Task<PagedResult<WalletTransaction>> GetByWalletIdPaginatedAsync(
        Guid walletId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(t => t.WalletId == walletId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt);

        return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
    }

    public async Task<PagedResult<WalletTransaction>> GetAllTransactionsAsync(
        int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.WalletTransactions
            .AsNoTracking()
            .Where(tx => !tx.IsDeleted)
            .OrderByDescending(w => w.CreatedAt);

        return await query
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public async Task<PagedResult<WalletTransaction>> GetByUserIdAsync(
     Guid userId,
     int pageIndex,
     int pageSize,
     CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt);

        return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
    }

    public async Task<IEnumerable<WalletTransaction>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.OrderId == orderId && !t.IsDeleted)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<WalletTransaction?> GetByOrderIdSingleAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.OrderId == orderId && !t.IsDeleted,
                cancellationToken);
    }

    public async Task<WalletTransaction?> GetByTransactionCodeAsync(
        string transactionCode,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.TransactionCode == transactionCode && !t.IsDeleted,
                cancellationToken);
    }

    // =======================
    // QUERY BY STATUS & TYPE
    // =======================

    public async Task<IEnumerable<WalletTransaction>> GetByStatusAsync(
        TransactionStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.Status == status && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransaction>> GetByTypeAsync(
        TransactionType type,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.TransactionType == type && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransaction>> GetByWalletAndStatusAsync(
        Guid walletId,
        TransactionStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.WalletId == walletId &&
                       t.Status == status &&
                       !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransaction>> GetPendingByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.WalletId == walletId &&
                       t.Status == TransactionStatus.Pending &&
                       !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // =======================
    // QUERY BY DATE RANGE
    // =======================

    public async Task<IEnumerable<WalletTransaction>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.CreatedAt >= fromDate &&
                       t.CreatedAt <= toDate &&
                       !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransaction>> GetByWalletAndDateRangeAsync(
        Guid walletId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.WalletId == walletId &&
                       t.CreatedAt >= fromDate &&
                       t.CreatedAt <= toDate &&
                       !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // =======================
    // BUSINESS VALIDATIONS
    // =======================

    public async Task<bool> ExistsByTransactionCodeAsync(
        string transactionCode,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(
                t => t.TransactionCode == transactionCode && !t.IsDeleted,
                cancellationToken);
    }

    public async Task<bool> HasPendingTransactionsAsync(
        Guid walletId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(
                t => t.WalletId == walletId &&
                     t.Status == TransactionStatus.Pending &&
                     !t.IsDeleted,
                cancellationToken);
    }

    public async Task<bool> IsOrderPaidAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(
                t => t.OrderId == orderId &&
                     t.TransactionType == TransactionType.Payment &&
                     t.Status == TransactionStatus.Completed &&
                     !t.IsDeleted,
                cancellationToken);
    }

    // =======================
    // STATUS UPDATES
    // =======================

    public async Task<bool> UpdateStatusAsync(
        Guid transactionId,
        TransactionStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _dbSet
            .FirstOrDefaultAsync(
                t => t.Id == transactionId && !t.IsDeleted,
                cancellationToken);

        if (transaction == null)
            return false;

        transaction.Status = newStatus;
        transaction.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CompleteTransactionAsync(
        Guid transactionId,
        string? completionNotes,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _dbSet
            .FirstOrDefaultAsync(
                t => t.Id == transactionId && !t.IsDeleted,
                cancellationToken);

        if (transaction == null)
            return false;

        transaction.Status = TransactionStatus.Completed;
        transaction.CompletedAt = DateTimeOffset.UtcNow;
        transaction.Notes = completionNotes;
        transaction.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> FailTransactionAsync(
        Guid transactionId,
        string failureReason,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _dbSet
            .FirstOrDefaultAsync(
                t => t.Id == transactionId && !t.IsDeleted,
                cancellationToken);

        if (transaction == null)
            return false;

        transaction.Status = TransactionStatus.Failed;
        transaction.FailureReason = failureReason;
        transaction.FailedAt = DateTimeOffset.UtcNow;
        transaction.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // =======================
    // STATISTICS & REPORTING
    // =======================

    public async Task<decimal> GetTotalAmountAsync(
        Guid walletId,
        TransactionType? type,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(t => t.WalletId == walletId &&
                       t.Status == TransactionStatus.Completed &&
                       !t.IsDeleted);

        if (type.HasValue)
        {
            query = query.Where(t => t.TransactionType == type.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= toDate.Value);
        }

        var total = await query.SumAsync(t => t.Amount, cancellationToken);
        return total;
    }

    public async Task<Dictionary<TransactionStatus, int>> GetCountByStatusAsync(
        Guid? walletId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(t => !t.IsDeleted);

        if (walletId.HasValue)
        {
            query = query.Where(t => t.WalletId == walletId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= toDate.Value);
        }

        return await query
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    public async Task<IEnumerable<WalletTransaction>> GetRecentTransactionsAsync(
        Guid walletId,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.WalletId == walletId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<WalletTransaction>> GetWithFilterAsync(
        Guid? walletId,
        Guid? userId,
        TransactionType? type,
        TransactionStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchKeyword,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(t => !t.IsDeleted);

        // Apply filters
        if (walletId.HasValue)
        {
            query = query.Where(t => t.WalletId == walletId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.TransactionType == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var keyword = searchKeyword.Trim().ToLower();
            query = query.Where(t =>
                t.TransactionCode.ToLower().Contains(keyword) ||
                (t.Description != null && t.Description.ToLower().Contains(keyword)) ||
                (t.Notes != null && t.Notes.ToLower().Contains(keyword)));
        }
        query = query.OrderByDescending(t => t.CreatedAt);

        return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
    }

    public async Task<WalletTransaction?> GetByReferenceAsync(
    Guid referenceId,
    WalletReferenceType referenceType,
    CancellationToken cancellationToken)
    {
        return await _context.WalletTransactions
            .FirstOrDefaultAsync(
                x => x.ReferenceId == referenceId &&
                     x.ReferenceType == referenceType &&
                     !x.IsDeleted,
                cancellationToken);
    }
}