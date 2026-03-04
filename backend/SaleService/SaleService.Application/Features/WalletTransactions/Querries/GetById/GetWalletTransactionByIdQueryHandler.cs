using Ardalis.Result;
using MediatR;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.WalletTransactions;

namespace SalesService.Application.Features.WalletTransactions.Queries.GetById;

public sealed class GetWalletTransactionByIdQueryHandler
    : IRequestHandler<GetWalletTransactionByIdQuery, Result<WalletTransactionResponse>>
{
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IIdentityServiceClient _identityServiceClient;

    public GetWalletTransactionByIdQueryHandler(
        IWalletTransactionRepository transactionRepository, IIdentityServiceClient identityServiceClien)
    {
        _transactionRepository = transactionRepository;
        _identityServiceClient = identityServiceClien;
    }

    public async Task<Result<WalletTransactionResponse>> Handle(
        GetWalletTransactionByIdQuery query,
        CancellationToken cancellationToken)
    {
        var userRole=await _identityServiceClient.GetUserByIdAsync(query.LoggedUserId);
        Console.WriteLine($"RoleName: '{userRole?.Role?.RoleName}'");
        Console.WriteLine($"Slug: '{userRole?.Role?.Slug}'");
        var transaction = await _transactionRepository.GetByIdAsync(
            query.TransactionId,
            cancellationToken);

        if (transaction == null)
        {
            return Result.NotFound("Transaction not found");
        }

        if (userRole != null)
        {
            Console.WriteLine("userRole not null");
        }
        else
        {
            Console.WriteLine("userRole is null");
        }

        var isOwner = query.LoggedUserId == transaction.UserId;
        var role = userRole?.Role?.RoleName?.Trim().ToLower();

        var isInternalUser = role == "administrator"
                        || role == "manager"
                        || role == "staff";

        if (!isOwner && !isInternalUser)
        {
            return Result.Forbidden("You do not have permission!!!");
        }


        var response = new WalletTransactionResponse
        {
            Id = transaction.Id,
            WalletId = transaction.WalletId.Value,
            UserId = transaction.UserId,

            TransactionCode = transaction.TransactionCode,
            Amount = transaction.Amount,
            TransactionType = transaction.TransactionType,
            Status = transaction.Status,

            BalanceBefore = transaction.BalanceBefore,
            BalanceAfter = transaction.BalanceAfter,

            PaymentMethod = transaction.PaymentMethod,
            Description = transaction.Description,
            Notes = transaction.Notes,

            ReferenceId = transaction.ReferenceId,
            ReferenceType = transaction.ReferenceType,
            OrderId = transaction.OrderId,

            ExternalTransactionId = transaction.ExternalTransactionId,

            FailureReason = transaction.FailureReason,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
            CompletedAt = transaction.CompletedAt,
            FailedAt = transaction.FailedAt,

            ParentTransactionId = transaction.ParentTransactionId
        };

        return Result.Success(response);
    }
}