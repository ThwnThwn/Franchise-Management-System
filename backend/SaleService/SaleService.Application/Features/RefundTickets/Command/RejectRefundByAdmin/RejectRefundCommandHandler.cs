using Ardalis.Result;
using MediatR;
using SalesService.Application.Features.RefundTickets.Command.RejectRefundByAdmin;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.RejectRefundByAdmin.RejectRefundCommand;

namespace SalesService.Application.Features.RefundTickets.Command.RejectRefund;

public sealed class RejectRefundCommandHandler
    : IRequestHandler<RejectRefundCommand, Result<RejectRefundResponse>>
{
    private readonly IRefundTicketRepository _refundTicketRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IIdentityServiceClient _identityServiceClient;

    public RejectRefundCommandHandler(
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

    public async Task<Result<RejectRefundResponse>> Handle(
    RejectRefundCommand command,
    CancellationToken cancellationToken)
    {
        var request = command.Request;

        var userRole = await _identityServiceClient
            .GetUserByIdAsync(command.InternalUserId);

        var role = userRole?.Role?.RoleName?.Trim().ToLower();

        var isAdmin = role == "administrator";

        if (!isAdmin)
        {
            return Result.Forbidden("Only Admin can reject refund requests");
        }

        var refundTicket = await _refundTicketRepository
            .GetByIdAsync(command.RefundTicketId, cancellationToken);

        if (refundTicket == null)
            return Result.NotFound("Refund ticket not found");

        if (refundTicket.Status != RefundTicketStatus.Pending)
            return Result.Invalid(new ValidationError(
                nameof(refundTicket.Status),
                $"Only pending refund tickets can be rejected. Current: {refundTicket.Status}"
            ));

        // Reject
        refundTicket.Status = RefundTicketStatus.Rejected;
        refundTicket.RejectedBy = command.InternalUserId;
        refundTicket.RejectedAt = DateTimeOffset.UtcNow;
        refundTicket.UpdatedAt = DateTimeOffset.UtcNow;
        refundTicket.RejectionReason = request.RejectNotes;

        await _refundTicketRepository.UpdateAsync(refundTicket, cancellationToken);

        var response = new RejectRefundResponse
        {
            RefundTicketId = refundTicket.Id,
            TicketCode = refundTicket.TicketCode,
            Status = refundTicket.Status,
            Amount = refundTicket.Amount,
            CustomerId = refundTicket.UserId,
            RejectdBy = command.InternalUserId,
            RejectdAt = refundTicket.RejectedAt!.Value,
            RejectNotes = request.RejectNotes,
            NextSteps = "Refund request rejected. No money will be returned."
        };

        return Result.Success(response);
    }

}