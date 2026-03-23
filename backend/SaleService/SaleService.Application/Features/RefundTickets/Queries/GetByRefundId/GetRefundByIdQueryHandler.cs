using Ardalis.Result;
using DocumentFormat.OpenXml.VariantTypes;
using MediatR;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;

namespace SalesService.Application.Features.RefundTickets.Queries.GetById;

public sealed class GetRefundByIdQueryHandler
    : IRequestHandler<GetRefundByIdQuery, Result<GetRefundByIdResponse>>
{
    private readonly IRefundTicketRepository _refundTicketRepository;
    private readonly IIdentityServiceClient _identityServiceClient;

    public GetRefundByIdQueryHandler(IRefundTicketRepository refundTicketRepository, IIdentityServiceClient identityServiceClient)
    {
        _refundTicketRepository = refundTicketRepository;
        _identityServiceClient = identityServiceClient;
    }

    public async Task<Result<GetRefundByIdResponse>> Handle(
        GetRefundByIdQuery request,
        CancellationToken cancellationToken)
    {

        var ticket = await _refundTicketRepository.GetByIdAsync(request.RefundTicketId, cancellationToken);

        //var userRole = await _identityServiceClient.GetUserByIdAsync(request.UserId);
        //var isOwner = ticket != null && ticket.UserId == request.UserId;
        //var isAdmin = userRole.Role.RoleName is "Administrator" or "Manager";
        //if (!isAdmin && !isOwner)
        //{
        //    return Result.Forbidden("You do not have permission!!!");
        //}

        if (ticket == null)
            return Result.NotFound($"Refund ticket not found for id '{request.RefundTicketId}'");
        if (ticket.WalletId == null || ticket.OriginalTransactionId == null)
        {
            throw new ArgumentException(nameof(ticket.WalletId) + nameof(ticket.OriginalTransactionId));
        }
        return Result.Success(new GetRefundByIdResponse
        {
            Id = ticket.Id,
            TicketCode = ticket.TicketCode,
            Amount = ticket.Amount,
            Reason = ticket.Reason ?? "No reason provided",
            WalletId = ticket.WalletId.Value,
            TransactionId = ticket.TransactionId,
            OriginalTransactionId = ticket.OriginalTransactionId.Value,
            Status = ticket.Status,
            CreatedAt = ticket.CreatedAt
        });
    }
}
