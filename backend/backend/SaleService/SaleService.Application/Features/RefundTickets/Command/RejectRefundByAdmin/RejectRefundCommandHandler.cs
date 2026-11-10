using Ardalis.Result;
using MediatR;
using SalesService.Application.Features.RefundTickets.Command.RejectRefundByAdmin;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.RejectRefundByAdmin.RejectRefundCommand;

namespace SalesService.Application.Features.RefundTickets.Command.RejectRefund;

public sealed class RejectRefundCommandHandler
    : IRequestHandler<RejectRefundCommand, Result<RejectRefundResponse>>
{
    private readonly IRefundTicketRepository _refundTicketRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;

    public RejectRefundCommandHandler(
        IRefundTicketRepository refundTicketRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository)
    {
        _refundTicketRepository = refundTicketRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<RejectRefundResponse>> Handle(
    RejectRefundCommand command,
    CancellationToken cancellationToken)
    {
        var request = command.Request;

        var refundTicket = await _refundTicketRepository.GetByTicketIdAsync(
            command.RefundTicketId,
            cancellationToken);


        if (refundTicket == null)
        {
            return Result.NotFound("Refund ticket not found");
        }


        if (refundTicket.Status != RefundTicketStatus.Pending)
            return Result.Invalid(new ValidationError(
                nameof(refundTicket.Status),
                $"Only pending refund tickets can be rejected. Current: {refundTicket.Status}"
            ));

        // Reject
        refundTicket.Status = RefundTicketStatus.Rejected;
        refundTicket.RejectedAt = DateTimeOffset.UtcNow;
        refundTicket.UpdatedAt = DateTimeOffset.UtcNow;
        refundTicket.RejectionReason = request.RejectNotes;

        var wallet = await _walletRepository.GetByUserIdAsync(refundTicket.UserId, cancellationToken);
        if (wallet == null)
        {
            return Result.Error("Customer wallet not found");
        }
        wallet.Balance += refundTicket.Amount;
        await _walletRepository.UpdateAsync(wallet, cancellationToken);
        var transaction= new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            UserId = refundTicket.UserId,
            Amount = +refundTicket.Amount,
            TransactionType = TransactionType.Refund,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Notes = refundTicket.Notes,
            Status = TransactionStatus.Completed,
            TransactionCode = GenerateTransactionCode(TransactionType.Refund)
        };
        await _transactionRepository.AddAsync(transaction, cancellationToken);
        refundTicket.TransactionId=transaction.Id;
        await _refundTicketRepository.UpdateAsync(refundTicket, cancellationToken);

        var response = new RejectRefundResponse
        {
            RefundTicketId = refundTicket.Id,
            TicketCode = refundTicket.TicketCode,
            Status = refundTicket.Status,
            Amount = refundTicket.Amount,
            CustomerId = refundTicket.UserId,
            RejectdAt = refundTicket.RejectedAt!.Value,
            RejectNotes = request.RejectNotes,
            NextSteps = "Refund request rejected. Your balance has been restored."
        };

        return Result.Success(response);
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