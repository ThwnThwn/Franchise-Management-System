using Ardalis.Result;
using MediatR;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;
using SalesService.Domain.Entities;
using static SalesService.Application.Features.RefundTickets.Queries.GetByStatus.GetRefundByStatusQuery;

namespace SalesService.Application.Features.RefundTickets.Queries.GetByStatus;

public sealed class GetRefundByStatusQueryHandler
    : IRequestHandler<GetRefundByStatusQuery, Result<BuildingBlocks.Core.Pagination.PagedResult<RefundTicketResponse>>>
{
    private readonly IRefundTicketRepository _refundTicketRepository;
    private readonly IIdentityServiceClient _identityServiceClient;

    public GetRefundByStatusQueryHandler(
        IRefundTicketRepository refundTicketRepository,
        IIdentityServiceClient identityServiceClient)
    {
        _refundTicketRepository = refundTicketRepository;
        _identityServiceClient = identityServiceClient;
    }

    public async Task<Result<BuildingBlocks.Core.Pagination.PagedResult<RefundTicketResponse>>> Handle(
        GetRefundByStatusQuery query,
        CancellationToken cancellationToken)
    {
        if (query.PageIndex < 1)
        {
            return Result.Invalid(new ValidationError(
                nameof(query.PageIndex),
                "Page index must be greater than 0"));
        }

        if (query.PageSize < 1 || query.PageSize > 100)
        {
            return Result.Invalid(new ValidationError(
                nameof(query.PageSize),
                "Page size must be between 1 and 100"));
        }


        //  Case 1: UserId is NULL → Admin viewing all users
        //if (!query.UserId.HasValue)
        //{
        //    // Must be admin (checked at controller level via [Authorize(Roles = "Admin")])
        //    // Or you can check here:
        //    var InternalRole = await _identityServiceClient.GetUserByIdAsync(query.UserId.Value);
        //    if (InternalRole?.Role.Slug is not ("admin" or "manager"))
        //        return Result.Forbidden("Only administrators or managers can view all refund tickets");
        //}
        ////  Case 2: UserId provided → User viewing own tickets OR admin viewing specific user
        //else
        //{
        //    var userRole = await _identityServiceClient.GetUserByIdAsync(query.UserId.Value);
        //    var role = userRole?.Role?.RoleName?.Trim().ToLower();

        //    var isInternalUser = role == "administrator"
        //                    || role == "manager";

        //    if (!isInternalUser)
        //    {
        //        return Result.Forbidden("Only Admin or Manager can get refund requests");
        //    }

        //}

        BuildingBlocks.Core.Pagination.PagedResult<RefundTicket> refundTickets;

        if (query.UserId.HasValue)
        {
            // ✅ Get refunds for specific user
            refundTickets = await _refundTicketRepository.GetByUserIdAndStatusAsync(
                query.UserId.Value,
                query.Status,
                query.PageIndex,
                query.PageSize,
                cancellationToken);
        }
        else
        {
            // ✅ Get all refunds (admin only)
            refundTickets = await _refundTicketRepository.GetByStatusAsync(
                query.Status,
                query.PageIndex,
                query.PageSize,
                cancellationToken);
        }

        // ===== STEP 4: MAP TO RESPONSE =====
        var responses = refundTickets.Items.Select(MapToResponse).ToList();

        // ✅ FIXED: Correct parameter order
        var pagedResponse = new BuildingBlocks.Core.Pagination.PagedResult<RefundTicketResponse>(
            responses,                    // items
            refundTickets.TotalCount,    // ✅ totalCount (not TotalPages)
            refundTickets.Page,          // ✅ pageIndex
            refundTickets.PageSize       // ✅ pageSize
        );

        return Result.Success(pagedResponse);
    }

    private static RefundTicketResponse MapToResponse(RefundTicket ticket)
    {
        return new RefundTicketResponse
        {
            Id = ticket.Id,
            TicketCode = ticket.TicketCode,
            UserId = ticket.UserId,
            WalletId = ticket.WalletId,
            OrderId = ticket.OrderId,
            OriginalTransactionId = ticket.OriginalTransactionId,

            // Amounts
            Amount = ticket.Amount,
            RefundAmount = ticket.RefundAmount,

            // Status & Reason
            Status = ticket.Status,
            Reason = ticket.Reason,
            RefundReasonType = ticket.RefundReason,

            // Timestamps
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,

            // ✅ Convert Guid.Empty to null for cleaner API response
            ApprovedBy = ticket.ApprovedBy != Guid.Empty ? ticket.ApprovedBy : null,
            ApprovedAt = ticket.ApprovedAt,
            ApprovalNotes = ticket.ApprovalNotes,

            RejectedBy = ticket.RejectedBy != Guid.Empty ? ticket.RejectedBy : null,
            RejectedAt = ticket.RejectedAt,
            RejectionReason = ticket.RejectionReason,

            CompletedAt = ticket.CompletedAt,
            TransactionId = ticket.TransactionId,

            CancelledBy = ticket.CancelledBy != Guid.Empty ? ticket.CancelledBy : null,
            CancelledAt = ticket.CancelledAt,
            CancellationReason = ticket.CancellationReason
        };
    }
}