using Ardalis.Result;
using MediatR;
using SalesService.Application.Authorization;
using SalesService.Application.Common.Models;
using SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund.InternalUserCreateRefundTicketCommand;
namespace SalesService.Application.Features.RefundTickets.Command.CreateRefund;

public sealed class InternalUserCreateRefundTicketCommandHandler
    : IRequestHandler<InternalUserCreateRefundTicketCommand, Result<InternalUserCreateRefundTicketResponse>>
{
    private readonly IRefundTicketRepository _refundTicketRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IRefundAuthorizationService _authorizationService;
    private readonly IIdentityServiceClient _identityServiceClient;
    public InternalUserCreateRefundTicketCommandHandler(
        IRefundTicketRepository refundTicketRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IRefundAuthorizationService authorizationService,
        IIdentityServiceClient identityServiceClient)
    {
        _refundTicketRepository = refundTicketRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _authorizationService = authorizationService;
        _identityServiceClient = identityServiceClient;
    }

    public async Task<Result<InternalUserCreateRefundTicketResponse>> Handle(
        InternalUserCreateRefundTicketCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var InternalUserOverride = command.InternalUserOverride;

        var userRole = await _identityServiceClient.GetUserByIdAsync(command.UserId);

        if (InternalUserOverride != null)
        {
            var OriginalTransaction = await _transactionRepository.GetByIdAsync(
                request.OriginalTransactionId,
                cancellationToken);

            if (OriginalTransaction == null)
                return Result.NotFound("Original transaction not found");

            // ✅ Call authorization service
            //var authResult = _authorizationService.ValidateInternalUserOverridePermissions(
            //    userRole.Role.RoleName,
            //    InternalUserOverride,
            //    request.Amount,
            //    OriginalTransaction.Amount,
            //    OriginalTransaction.CreatedAt);

            //if (!authResult.IsSuccess)
            //    return Result<InternalUserCreateRefundTicketResponse>.Forbidden(authResult.Errors.ToArray());
        }
        // ===== BASIC VALIDATIONS (Always Required) =====

        if (request.Amount <= 0)
            return Result.Invalid(new ValidationError(
                nameof(request.Amount),
                "Amount must be greater than zero"));

        // ✅ Validate wallet exists
        var wallet = await _walletRepository.GetByIdAsync(
            request.WalletId,
            cancellationToken);

        if (wallet == null)
            return Result.NotFound("Wallet not found");

        // ✅ Validate wallet belongs to target user
        if (wallet.UserId != command.UserId)
            return Result.Invalid(new ValidationError(
                nameof(request.WalletId),
                "Wallet does not belong to target user"));

        // ✅ Validate wallet status
        if (wallet.Status != WalletStatus.Active)
            return Result.Invalid(new ValidationError(
                nameof(request.WalletId),
                "Wallet is not active"));

        // ✅ Validate original transaction exists
        var originalTransaction = await _transactionRepository.GetByIdAsync(
            request.OriginalTransactionId,
            cancellationToken);

        if (originalTransaction == null)
            return Result.NotFound("Original transaction not found");

        // ✅ Validate transaction belongs to wallet
        if (originalTransaction.WalletId != request.WalletId)
            return Result.Invalid(new ValidationError(
                nameof(request.OriginalTransactionId),
                "Transaction does not belong to this wallet"));

        // ✅ Validate transaction type
        if (originalTransaction.TransactionType != TransactionType.Payment)
            return Result.Invalid(new ValidationError(
                nameof(request.OriginalTransactionId),
                "Only payment transactions can be refunded"));

        // ✅ Validate transaction status
        if (originalTransaction.Status != TransactionStatus.Completed)
            return Result.Invalid(new ValidationError(
                nameof(request.OriginalTransactionId),
                "Only completed transactions can be refunded"));

        // ===== CONDITIONAL VALIDATIONS (Can be bypassed by InternalUser) =====

        // ✅ Time limit validation (unless bypassed)
        if (!InternalUserOverride.BypassTimeLimit)
        {
            var refundDeadline = originalTransaction.CreatedAt.AddDays(30);
            if (DateTimeOffset.UtcNow > refundDeadline)
                return Result.Invalid(new ValidationError(
                    nameof(request.OriginalTransactionId),
                    $"Refund window expired (deadline: {refundDeadline:yyyy-MM-dd}). " +
                    $"InternalUser can use BypassTimeLimit flag to override."));
        }

        // ✅ Check for existing pending refund
        if (await _refundTicketRepository.ExistingRefundWithPendingStatusAsync(
            request.OriginalTransactionId,
            cancellationToken))
            return Result.Invalid(new ValidationError(
                nameof(request.OriginalTransactionId),
                "A pending refund already exists for this transaction"));

        // ✅ Amount validation (unless bypassed)
        decimal totalRefunded = 0;
        decimal remainingRefundable = originalTransaction.Amount;

        if (!InternalUserOverride.BypassAmountValidation)
        {
            totalRefunded = await _refundTicketRepository
                .GetTotalRefundedAmountByTransactionIdAsync(
                    request.OriginalTransactionId,
                    cancellationToken);

            remainingRefundable = originalTransaction.Amount - totalRefunded;

            if (request.Amount > remainingRefundable)
                return Result.Invalid(new ValidationError(
                    nameof(request.Amount),
                    $"Refund amount exceeds remaining refundable amount. " +
                    $"Original: {originalTransaction.Amount:N2}, " +
                    $"Already refunded: {totalRefunded:N2}, " +
                    $"Remaining: {remainingRefundable:N2}. " +
                    $"InternalUser can use BypassAmountValidation flag to override."));
        }
        else
        {
            // ✅ Even with bypass, get total for audit purposes
            totalRefunded = await _refundTicketRepository
                .GetTotalRefundedAmountByTransactionIdAsync(
                    request.OriginalTransactionId,
                    cancellationToken);
            remainingRefundable = originalTransaction.Amount - totalRefunded;
        }

        // ===== CREATE REFUND TICKET =====

        // ✅ Generate unique ticket code
        var ticketCode = GenerateTicketCode();

        // ✅ Determine initial status based on AutoApprove flag
        var initialStatus = InternalUserOverride.AutoApprove
            ? RefundTicketStatus.Approved
            : RefundTicketStatus.Pending;

        // ✅ Build audit notes
        var auditNotes = BuildAuditNotes(
            InternalUserOverride,
            totalRefunded,
            remainingRefundable,
            originalTransaction.Amount);

        // ✅ Create refund ticket entity
        var refundTicket = new RefundTicket
        {
            Id = Guid.NewGuid(),
            TicketCode = ticketCode,
            Amount = request.Amount,
            RefundAmount = request.Amount,
            Reason = request.Reason,
            WalletId = request.WalletId,
            UserId = command.UserId,
            OrderId = originalTransaction.OrderId,
            OriginalTransactionId = request.OriginalTransactionId,
            Status = initialStatus,

            // ✅ InternalUser-specific fields
            RefundReason = InternalUserOverride.RefundReason,
            InternalNotes = InternalUserOverride.InternalNotes,
            CreatedByInternalUserId = InternalUserOverride.InternalUserId,

            // ✅ Audit trail
            Notes = auditNotes,

            // ✅ Auto-approval fields
            ApprovedBy = InternalUserOverride.AutoApprove ? InternalUserOverride.InternalUserId : null,
            ApprovedAt = InternalUserOverride.AutoApprove ? DateTime.UtcNow : null,
            ApprovalNotes = InternalUserOverride.AutoApprove
                ? $"Auto-approved by InternalUser. Reason: {InternalUserOverride.RefundReason}"
                : null,

            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // ✅ Save to repository
        var refundTicketId = await _refundTicketRepository.AddAsync(
            refundTicket,
            cancellationToken);

        if (refundTicketId == Guid.Empty)
            return Result.Error("Failed to create refund ticket");

        // ✅ TODO: Send notifications
        // - Notify user about refund creation
        // - Notify InternalUser team for approval queue (if not auto-approved)
        // - Log to audit system

        // ✅ Map to response
        var response = new InternalUserCreateRefundTicketResponse
        {
            Id = refundTicket.Id,
            TicketCode = refundTicket.TicketCode,
            Amount = refundTicket.Amount,
            Reason = refundTicket.Reason,
            WalletId = refundTicket.WalletId,
            OrderId = refundTicket.OrderId,
            TransactionId = refundTicket.TransactionId,
            OriginalTransactionId = refundTicket.OriginalTransactionId,
            Status = refundTicket.Status,
            CreatedAt = refundTicket.CreatedAt
        };

        return Result.Success(response);
    }

    // ===== PRIVATE HELPER METHODS =====

    private static string GenerateTicketCode()
    {
        var shortGuid = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        return $"RFT-ADM-{timestamp}-{shortGuid}"; // ✅ "ADM" prefix for InternalUser-created
    }

    private static string BuildAuditNotes(
        InternalUserOverrideInfo InternalUserOverride,
        decimal totalRefunded,
        decimal remainingRefundable,
        decimal originalAmount)
    {
        var notes = new System.Text.StringBuilder();

        notes.AppendLine($"[InternalUser REFUND] Created by InternalUser: {InternalUserOverride.InternalUserId}");
        notes.AppendLine($"Original Transaction Amount: {originalAmount:N2}");
        notes.AppendLine($"Previously Refunded: {totalRefunded:N2}");
        notes.AppendLine($"Remaining Refundable: {remainingRefundable:N2}");

        if (InternalUserOverride.AutoApprove)
            notes.AppendLine("✓ Auto-Approved");

        if (InternalUserOverride.BypassTimeLimit)
            notes.AppendLine("⚠ BYPASS: Time Limit");

        if (InternalUserOverride.BypassAmountValidation)
            notes.AppendLine("⚠ BYPASS: Amount Validation");

        if (InternalUserOverride.RefundReason.HasValue)
            notes.AppendLine($"Reason Type: {InternalUserOverride.RefundReason.Value}");

        if (!string.IsNullOrWhiteSpace(InternalUserOverride.InternalNotes))
            notes.AppendLine($"InternalUser Notes: {InternalUserOverride.InternalNotes}");

        return notes.ToString();
    }
}