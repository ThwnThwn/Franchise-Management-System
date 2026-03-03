using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Authorization;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.Wallets.Commands.UpdateStatus.UpdateWalletStatusCommand;

namespace SalesService.Application.Features.Wallets.Commands.UpdateStatus;

public sealed class UpdateWalletStatusCommandHandler
    : IRequestHandler<UpdateWalletStatusCommand, Result<UpdateWalletStatusResponse>>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IRefundTicketRepository _refundRepository;
    private readonly IWalletAuthorizationService _authService;
    private readonly IIdentityServiceClient _identityServiceClient;

    public UpdateWalletStatusCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IRefundTicketRepository refundRepository,
        IWalletAuthorizationService authService,
        IIdentityServiceClient identityService)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _refundRepository = refundRepository;
        _authService = authService;
        _identityServiceClient = identityService;
    }

    public async Task<Result<UpdateWalletStatusResponse>> Handle(
        UpdateWalletStatusCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var userRole = await _identityServiceClient.GetUserByIdAsync(command.AdminId);
        var authResult = _authService.ValidateStatusUpdatePermission(
            userRole?.Role?.RoleName,
            request.NewStatus);

        if (!authResult.IsSuccess)
            return Result<UpdateWalletStatusResponse>.Forbidden(authResult.Errors.ToArray());

        // ✅ Step 2: Get wallet
        var wallet = await _walletRepository.GetForUpdateAsync(
            command.WalletId,
            cancellationToken);

        if (wallet == null)
        {
            return Result.NotFound($"Wallet {command.WalletId} not found");
        }

        // ✅ Step 3: Idempotent check - same status is success
        if (wallet.Status == request.NewStatus)
        {
            return Result.Success(new UpdateWalletStatusResponse
            {
                WalletId = wallet.Id,
                OldStatus = wallet.Status,
                NewStatus = wallet.Status,
                Reason = request.Reason,
                UpdatedBy = command.AdminId,
                UpdatedAt = wallet.UpdatedAt
            });
        }

        // ✅ Step 4: Validate state transition (Business logic in Handler)
        var transitionValid = IsValidStatusTransition(wallet.Status, request.NewStatus);
        if (!transitionValid)
        {
            return Result.Invalid(new ValidationError(
                nameof(request.NewStatus),
                $"Invalid status transition from {wallet.Status} to {request.NewStatus}"));
        }

        // ✅ Step 5: Business rules - Check pending issues (Business logic in Handler)
        if (request.NewStatus == WalletStatus.Locked)
        {
            var hasPendingTxn = await _transactionRepository.HasPendingTransactionsAsync(
                wallet.Id,
                cancellationToken);

            var hasPendingRefunds = await _refundRepository.ExistingRefundWithPendingStatusAsync(
                wallet.Id,
                cancellationToken);

            if (hasPendingTxn || hasPendingRefunds)
            {
                return Result.Invalid(new ValidationError(
                    "PendingIssues",
                    "Cannot lock wallet with pending transactions or refunds. " +
                    "Please resolve pending issues first."));
            }
        }

        // ✅ Step 6: Update wallet status
        var oldStatus = wallet.Status;
        wallet.Status = request.NewStatus;
        wallet.UpdatedAt = DateTimeOffset.UtcNow;

        // ✅ Optional: Add status change to audit log/notes
        var statusChangeLog = BuildStatusChangeLog(
            oldStatus,
            request.NewStatus,
            command.AdminId,
            request.Reason,
            request.AdminNotes);

        wallet.Notes = string.IsNullOrEmpty(wallet.Notes)
            ? statusChangeLog
            : $"{wallet.Notes}\n{statusChangeLog}";

        try
        {
            // ✅ Step 7: Save changes
            await _walletRepository.UpdateAsync(wallet, cancellationToken);

            // ✅ Step 8: Return response
            var response = new UpdateWalletStatusResponse
            {
                WalletId = wallet.Id,
                OldStatus = oldStatus,
                NewStatus = wallet.Status,
                Reason = request.Reason,
                UpdatedBy = command.AdminId,
                UpdatedAt = wallet.UpdatedAt
            };

            return Result.Success(response);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Error(
                "Wallet was modified by another transaction. Please retry.");
        }
        catch (Exception ex)
        {
            return Result.Error($"Failed to update wallet status: {ex.Message}");
        }
    }

    // ===== PRIVATE HELPERS - Business Logic =====

    /// <summary>
    /// ✅ Business logic: Validate state transitions
    /// </summary>
    private static bool IsValidStatusTransition(
        WalletStatus current,
        WalletStatus next)
    {
        if (current == next)
            return true;

        return current switch
        {
            WalletStatus.Inactive => next == WalletStatus.Active,
            
            WalletStatus.Active => next is WalletStatus.Locked 
                                          or WalletStatus.Suspended,
            
            WalletStatus.Locked => next is WalletStatus.Active 
                                          or WalletStatus.Suspended,
            
            WalletStatus.Suspended => next == WalletStatus.Active,
            
            _ => false
        };
    }

    /// <summary>
    /// Build audit log entry for status change
    /// </summary>
    private static string BuildStatusChangeLog(
        WalletStatus oldStatus,
        WalletStatus newStatus,
        Guid adminId,
        string? reason,
        string? adminNotes)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var log = $"[{timestamp}] Status changed: {oldStatus} → {newStatus} by Admin {adminId}";

        if (!string.IsNullOrWhiteSpace(reason))
            log += $"\nReason: {reason}";

        if (!string.IsNullOrWhiteSpace(adminNotes))
            log += $"\nNotes: {adminNotes}";

        return log;
    }
}