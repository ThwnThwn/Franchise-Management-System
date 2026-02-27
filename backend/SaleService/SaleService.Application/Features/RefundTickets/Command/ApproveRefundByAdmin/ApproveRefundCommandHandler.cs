using Ardalis.Result;
using MediatR;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.ApproveRefund.ApproveRefundCommand;

namespace SalesService.Application.Features.RefundTickets.Command.ApproveRefund;

public sealed class ApproveRefundCommandHandler
    : IRequestHandler<ApproveRefundCommand, Result<ApproveRefundResponse>>
{
    private readonly IRefundTicketRepository _refundTicketRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IIdentityServiceClient _identityServiceClient;

    public ApproveRefundCommandHandler(
        IRefundTicketRepository refundTicketRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IIdentityServiceClient identityServiceClient)
    {
        _refundTicketRepository = refundTicketRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _identityServiceClient = identityServiceClient;
    }

    public async Task<Result<ApproveRefundResponse>> Handle(
        ApproveRefundCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;


        //var userRole = await _identityServiceClient.GetUserByIdAsync(command.InternalUserId);
        //var role = userRole?.Role?.RoleName?.Trim().ToLower();

        //var isInternalUser = role == "administrator"
        //                || role == "manager"
        //                || role == "staff";

        //if (!isInternalUser)
        //{
        //    return Result.Forbidden("Only Admin or Manager can approve refund requests");
        //}

        // ===== STEP 2: LOAD REFUND TICKET =====

        Console.WriteLine($"[Reject] Looking for RefundTicketId: {command.RefundTicketId}");
        var refundTicket = await _refundTicketRepository.GetByTicketIdAsync(
            command.RefundTicketId,
            cancellationToken);
        Console.WriteLine($"[Reject] Result: {(refundTicket == null ? "NULL" : refundTicket.Id.ToString())}");
        

        if (refundTicket == null)
        {
            return Result.NotFound("Refund ticket not found");
        }

        // ===== STEP 3: VALIDATE STATUS =====
        if (refundTicket.Status != RefundTicketStatus.Pending)
        {
            return Result.Invalid(new ValidationError(
                nameof(refundTicket.Status),
                $"Only pending refund tickets can be approved. " +
                $"Current status: {refundTicket.Status}"));
        }

        // ===== STEP 4: VALIDATE WALLET EXISTS & ACTIVE =====
        var wallet = await _walletRepository.GetByIdAsync(
            refundTicket.WalletId,
            cancellationToken);

        if (wallet == null)
        {
            return Result.NotFound("Associated wallet not found");
        }

        if (wallet.Status != WalletStatus.Active)
        {
            return Result.Invalid(new ValidationError(
                "WalletStatus",
                $"Wallet is not active. Cannot process refund. " +
                $"Current status: {wallet.Status}"));
        }

        // ===== STEP 5: VALIDATE ORIGINAL TRANSACTION =====
        var originalTransaction = await _transactionRepository.GetByIdAsync(
            refundTicket.OriginalTransactionId,
            cancellationToken);

        if (originalTransaction == null)
        {
            return Result.Error("Original transaction not found. Cannot approve refund.");
        }

        if (originalTransaction.Status != TransactionStatus.Completed)
        {
            return Result.Invalid(new ValidationError(
                "OriginalTransaction",
                "Original transaction is not completed. Cannot approve refund."));
        }

        // ===== STEP 6: VALIDATE REFUND AMOUNT =====
        var originalAmount = Math.Abs(originalTransaction.Amount);

        var totalRefunded = await _refundTicketRepository
            .GetTotalRefundedAmountByTransactionIdAsync(
                originalTransaction.Id,
                cancellationToken);

        var remainingRefundable = originalAmount - totalRefunded;

        if (refundTicket.Amount > remainingRefundable)
        {
            return Result.Invalid(new ValidationError(
                "RefundAmount",
                $"Refund amount exceeds remaining refundable amount. " +
                $"This ticket may have been approved already."));
        }

        // ===== STEP 7: APPROVE REFUND =====
        refundTicket.Status = RefundTicketStatus.Approved;
        refundTicket.ApprovedBy = command.InternalUserId;
        refundTicket.ApprovedAt = DateTime.UtcNow;
        refundTicket.ApprovalNotes = request.ApprovalNotes;
        refundTicket.UpdatedAt = DateTimeOffset.UtcNow;

        await _refundTicketRepository.UpdateAsync(
            refundTicket,
            cancellationToken);

        string nextSteps = "Refund approved. Awaiting processing to add money to wallet.";
        

        // ===== STEP 9: RETURN RESPONSE =====
        var response = new ApproveRefundResponse
        {
            RefundTicketId = refundTicket.Id,
            TicketCode = refundTicket.TicketCode,
            Status = refundTicket.Status,
            Amount = refundTicket.Amount,
            CustomerId = refundTicket.UserId,
            ApprovedBy = command.InternalUserId,
            ApprovedAt = refundTicket.ApprovedAt!.Value,
            ApprovalNotes = refundTicket.ApprovalNotes,
            NextSteps = nextSteps
        };

        return Result.Success(response);
    }
}