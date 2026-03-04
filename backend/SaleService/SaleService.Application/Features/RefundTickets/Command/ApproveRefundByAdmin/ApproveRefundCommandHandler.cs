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


        var refundTicket = await _refundTicketRepository.GetByTicketIdAsync(
            command.RefundTicketId,
            cancellationToken);

        if (refundTicket == null)
        {
            return Result.NotFound("Refund ticket not found");
        }

        //  VALIDATE STATUS 
        if (refundTicket.Status != RefundTicketStatus.Pending)
        {
            return Result.Invalid(new ValidationError(
                nameof(refundTicket.Status),
                $"Only pending refund tickets can be approved. " +
                $"Current status: {refundTicket.Status}"));
        }

        // VALIDATE WALLET EXISTS & ACTIVE
        var wallet = await _walletRepository.GetByUserIdAsync(
            refundTicket.UserId,
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

        if (refundTicket.Amount > wallet.Balance)
        {
            return Result.Invalid(new ValidationError(
                "RefundAmount",
                $"Refund amount exceeds remaining balance in wallet."));
        }

        refundTicket.Status = RefundTicketStatus.Approved;
        refundTicket.ApprovedAt = DateTime.UtcNow;
        refundTicket.ApprovalNotes = request.ApprovalNotes;
        refundTicket.UpdatedAt = DateTimeOffset.UtcNow;

        await _refundTicketRepository.UpdateAsync(
            refundTicket,
            cancellationToken);

        string nextSteps = "Refund approved. Awaiting processing to add money to wallet.";
        

        var response = new ApproveRefundResponse
        {
            RefundTicketId = refundTicket.Id,
            TicketCode = refundTicket.TicketCode,
            Status = refundTicket.Status,
            Amount = refundTicket.Amount,
            CustomerId = refundTicket.UserId,
            ApprovedAt = refundTicket.ApprovedAt!.Value,
            ApprovalNotes = refundTicket.ApprovalNotes,
            NextSteps = nextSteps
        };

        return Result.Success(response);
    }
}