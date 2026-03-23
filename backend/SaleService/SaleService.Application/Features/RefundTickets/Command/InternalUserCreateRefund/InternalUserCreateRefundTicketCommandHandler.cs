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
    public InternalUserCreateRefundTicketCommandHandler(
        IRefundTicketRepository refundTicketRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository)
    {
        _refundTicketRepository = refundTicketRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<InternalUserCreateRefundTicketResponse>> Handle(
    InternalUserCreateRefundTicketCommand command,
    CancellationToken cancellationToken)
    {
        var request = command.Request;
        var internalUserOverride = command.InternalUserOverride;

        if( internalUserOverride == null )
        {
            throw new ArgumentNullException("internalUserOverride is empty internalUserOverrideHanler");
        }

        if (request.Amount <= 0)
            return Result.Invalid(new ValidationError(
                nameof(request.Amount),
                "Amount must be greater than zero"));

        var wallet = await _walletRepository.GetByUserIdAsync(
            command.UserId, cancellationToken);

        if (wallet == null)
            return Result.NotFound("Wallet not found");

        if (wallet.Status != WalletStatus.Active)
            return Result.Invalid(new ValidationError(
                nameof(wallet.Id),
                "Wallet is not active"));

        // ✅ Không cần originalTransaction, chỉ validate amount vs wallet balance
        if (!internalUserOverride.BypassAmountValidation)
        {
            if (request.Amount > wallet.Balance)
                return Result.Invalid(new ValidationError(
                    nameof(request.Amount),
                    $"Refund amount ({request.Amount:N2}) exceeds wallet balance ({wallet.Balance:N2}). " +
                    $"InternalUser can use BypassAmountValidation flag to override."));
        }

        var ticketCode = GenerateTicketCode();

        var initialStatus = internalUserOverride.AutoApprove
            ? RefundTicketStatus.Approved
            : RefundTicketStatus.Pending;

        var auditNotes = BuildAuditNotes(internalUserOverride, wallet.Balance, request.Amount);

        var refundTicket = new RefundTicket
        {
            Id = Guid.NewGuid(),
            TicketCode = ticketCode,
            Amount = request.Amount,
            RefundAmount = request.Amount,
            Reason = request.Reason,
            WalletId = wallet.Id,
            UserId = command.UserId,
            Status = initialStatus,
            RefundReason = internalUserOverride.RefundReason,
            InternalNotes = internalUserOverride.InternalNotes,
            CreatedByInternalUserId = internalUserOverride.InternalUserId,
            Notes = auditNotes,
            ApprovedBy = internalUserOverride.AutoApprove ? internalUserOverride.InternalUserId : null,
            ApprovedAt = internalUserOverride.AutoApprove ? DateTime.UtcNow : null,
            ApprovalNotes = internalUserOverride.AutoApprove
                ? $"Auto-approved by InternalUser. Reason: {internalUserOverride.RefundReason}"
                : null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        if (internalUserOverride.AutoApprove)
            wallet.Balance -= request.Amount;
        await _walletRepository.UpdateAsync(wallet, cancellationToken);

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            UserId = refundTicket.UserId,
            Amount = +refundTicket.Amount,
            TransactionType = TransactionType.Refund,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Notes = refundTicket.Notes,
            TransactionCode = GenerateTransactionCode(TransactionType.Refund)
        };

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        refundTicket.TransactionId = transaction.Id;

        var refundTicketId = await _refundTicketRepository.AddAsync(refundTicket, cancellationToken);

        if (refundTicketId == Guid.Empty)
            return Result.Error("Failed to create refund ticket");

        return Result.Success(new InternalUserCreateRefundTicketResponse
        {
            Id = refundTicket.Id,
            TicketCode = refundTicket.TicketCode,
            Amount = refundTicket.Amount,
            Reason = refundTicket.Reason,
            TransactionId = refundTicket.TransactionId,
            Status = refundTicket.Status,
            CreatedAt = refundTicket.CreatedAt
        });
    }

    private static string BuildAuditNotes(
        InternalUserOverrideInfo internalUserOverride,
        decimal walletBalance,
        decimal refundAmount)
    {
        var notes = new System.Text.StringBuilder();
        notes.AppendLine($"[InternalUser REFUND] Created by: {internalUserOverride.InternalUserId}");
        notes.AppendLine($"Wallet Balance: {walletBalance:N2}");
        notes.AppendLine($"Refund Amount: {refundAmount:N2}");

        if (internalUserOverride.AutoApprove)
            notes.AppendLine("✓ Auto-Approved");
        if (internalUserOverride.BypassAmountValidation)
            notes.AppendLine("⚠ BYPASS: Amount Validation");
        if (internalUserOverride.BypassTimeLimit)
            notes.AppendLine("⚠ BYPASS: Time Limit");
        if (internalUserOverride.RefundReason.HasValue)
            notes.AppendLine($"Reason Type: {internalUserOverride.RefundReason.Value}");
        if (!string.IsNullOrWhiteSpace(internalUserOverride.InternalNotes))
            notes.AppendLine($"InternalUser Notes: {internalUserOverride.InternalNotes}");

        return notes.ToString();
    }

    private static string GenerateTicketCode()
    {
        var shortGuid = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        return $"RFT-ADM-{timestamp}-{shortGuid}"; // ✅ "ADM" prefix for InternalUser-created
    }

    private static string GenerateTransactionCode(TransactionType type)
    {
        var prefix = type switch
        {
            TransactionType.Deposit => "DEP",
            TransactionType.Withdrawal => "WDR",
            TransactionType.Payment => "PAY",
            TransactionType.Refund => "REF",
            TransactionType.Transfer => "TRF",
            _ => "TXN"
        };

        var shortGuid = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"{prefix}-{timestamp}-{shortGuid}";
    }
}