using Ardalis.Result;
using MediatR;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;

namespace SalesService.Application.Features.RefundTickets.Queries.GetAll;

public sealed class GetAllRefundsHandler
    : IRequestHandler<GetAllRefundsQuery, Result<BuildingBlocks.Core.Pagination.PagedResult<GetAllRefundsResponse>>>
{
    private readonly IRefundTicketRepository _ticketRepository;
    private readonly IIdentityServiceClient _identityServiceClient;

    public GetAllRefundsHandler(IRefundTicketRepository refundTicketRepository, IIdentityServiceClient identityServiceClient)
    {
        _ticketRepository = refundTicketRepository;
        _identityServiceClient = identityServiceClient;
    }

    public async Task<Result<BuildingBlocks.Core.Pagination.PagedResult<GetAllRefundsResponse>>> Handle(
        GetAllRefundsQuery request,
        CancellationToken cancellationToken)
    {
        var userRole = await _identityServiceClient.GetUserByIdAsync(request.InternalUserId);
        if (userRole.Role.Slug != "admin" && userRole.Role.Slug != "manager")
        {
            return Result.Forbidden("You do not have permission!!!");
        }

        var pagedResult = await _ticketRepository.GetAllRefundsAsync(
            request.PageIndex,
            request.PageSize,
            cancellationToken);

        if(pagedResult==null)
        {
            return Result.NotFound("No refund tickets found.");
        }

        var items = pagedResult.Items.Select(ticket => new GetAllRefundsResponse
        {
            Id = ticket.Id,
            TicketCode = ticket.TicketCode,
            Amount = ticket.Amount,
            RefundAmount = ticket.RefundAmount,

            Reason = ticket.Reason,
            RefundReason = ticket.RefundReason,

            WalletId = ticket.WalletId,
            UserId = ticket.UserId,

            OrderId = ticket.OrderId,
            TransactionId = ticket.TransactionId,
            OriginalTransactionId = ticket.OriginalTransactionId,

            Status = ticket.Status,
            RefundTicketMethod = ticket.RefundTicketMethod,

            ProcessedBy = ticket.ProcessedBy,
            ApprovedBy = ticket.ApprovedBy,
            RejectedBy = ticket.RejectedBy,
            CreatedByInternalUserId = ticket.CreatedByInternalUserId,

            ApprovalNotes = ticket.ApprovalNotes,
            RejectionReason = ticket.RejectionReason,
            InternalNotes = ticket.InternalNotes,
            Notes = ticket.Notes,

            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ApprovedAt = ticket.ApprovedAt,
            RejectedAt = ticket.RejectedAt,
            CancelledAt = ticket.CancelledAt,
            CompletedAt = ticket.CompletedAt
        }).ToList();

        return Result.Success(new BuildingBlocks.Core.Pagination.PagedResult<GetAllRefundsResponse>(
            items,
            pagedResult.TotalCount,
            request.PageIndex,
            request.PageSize));
    }
}
