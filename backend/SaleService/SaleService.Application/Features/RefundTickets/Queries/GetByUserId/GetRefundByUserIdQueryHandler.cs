using Ardalis.Result;
using BuildingBlocks.Core.Pagination;
using MediatR;
using SalesService.Application.Features.RefundTickets.Queries.GetAll;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;

namespace SalesService.Application.Features.RefundTickets.Queries.GetByUserId;

public sealed class GetRefundByUserIdHandler
    : IRequestHandler<GetRefundByUserIdQuery, Result<BuildingBlocks.Core.Pagination.PagedResult<GetRefundByUserIdResponse>>>
{
    private readonly IRefundTicketRepository _refundTicketRepository;
    private readonly IIdentityServiceClient _identityServiceClient;

    public GetRefundByUserIdHandler(IRefundTicketRepository refundTicketRepository, IIdentityServiceClient identityServiceClient)
    {
        _refundTicketRepository = refundTicketRepository;
        _identityServiceClient = identityServiceClient;
    }

    public async Task<Result<BuildingBlocks.Core.Pagination.PagedResult<GetRefundByUserIdResponse>>> Handle(
        GetRefundByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var tickets = await _refundTicketRepository.GetByUserIdAsync(request.UserIdToSearch, request.PageIndex,request.PageSize,cancellationToken);

        if (tickets == null)
            return Result.NotFound($"Refund ticket not found with '{request.UserIdToSearch}'");

        var userRole = await _identityServiceClient.GetUserByIdAsync(request.LoggedUserId);
        
        var isOwner = tickets != null && tickets.Items.First().UserId == request.LoggedUserId;
        var isAdmin = userRole?.Role?.Slug is "admin" or "manager";
        if (!isAdmin && !isOwner)
        {
            return Result.Forbidden("You do not have permission!!!");
        }   

       var response = tickets.Items.Select(ticket => new GetRefundByUserIdResponse
        {
            Id = ticket.Id,
            UserId = ticket.UserId,
            TicketCode = ticket.TicketCode,
            Amount = ticket.Amount,
            RefundAmount= ticket.RefundAmount,
            Reason = ticket.Reason,
            WalletId = ticket.WalletId,
            OrderId = ticket.OrderId,
            TransactionId = ticket.TransactionId,
            OriginalTransactionId = ticket.OriginalTransactionId,
            Status = ticket.Status,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
       }).ToList();
        return Result.Success(new BuildingBlocks.Core.Pagination.PagedResult<GetRefundByUserIdResponse>(
            response,
            tickets.TotalCount,
            request.PageIndex,
            request.PageSize));
    }

}
