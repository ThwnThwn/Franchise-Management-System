using Ardalis.Result;
using Grpc.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Features.Wallets.Command.AddBalance;
using SalesService.Application.Features.Wallets.Command.Create;
using SalesService.Application.Features.Wallets.Command.DeductBalance;
using SalesService.Application.Features.Wallets.Commands.UpdateStatus;
using SalesService.Application.Features.Wallets.Queries.GetByUserId;
using SalesService.Grpc;

namespace SalesService.API.Services.Wallets;

public sealed class WalletGrpcService : WalletGrpc.WalletGrpcBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WalletGrpcService> _logger;

    public WalletGrpcService(IMediator mediator, ILogger<WalletGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<CreateWalletResponse> CreateWallet(
        CreateWalletRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid user_id format"));

        var command = new CreateWalletCommand(userId);
        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                string.Join(", ", result.Errors)));

        var data = result.Value;

        return new CreateWalletResponse
        {
            WalletId = data.WalletId.ToString(),
            UserId = data.UserId.ToString(),
            Balance = data.Balance.ToString("F2"), // ✅ String for precision
            Status = MapDomainStatusToGrpc(data.Status),
            CreatedAt = data.CreatedAt.ToString("O")
        };
    }

    public override async Task<AddBalanceResponse> AddBalance(
        AddBalanceRequest request,
        ServerCallContext context)
    {
        // ✅ Only validate user_id (wallet lookup by user_id internally)
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid user_id format"));

        // ✅ Parse and validate amount
        if (!decimal.TryParse(request.Amount, out var amount) || amount <= 0)
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid amount. Must be a positive number"));

        // ✅ Retry loop for optimistic concurrency
        const int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                // Get current wallet (includes current row_version)
                var walletQuery = new GetWalletByUserIdQuery(userId);
                var walletResult = await _mediator.Send(walletQuery, context.CancellationToken);

                if (walletResult.Status == Ardalis.Result.ResultStatus.NotFound)
                    throw new RpcException(new Status(
                        StatusCode.NotFound,
                        "Wallet not found for user"));

                var wallet = walletResult.Value;

                // ✅ Create command with current row_version
                var command = new AddBalanceCommand(
                    wallet.Id,
                    userId,
                    new AddBalanceCommand.AddBalanceCommandRequest
                    {
                        Amount = amount,
                        RowVersion = wallet.RowVersion, // Current version from DB
                        Description = request.Description,
                        PaymentMethod = MapGrpcPaymentMethodToDomain(request.PaymentMethod),
                        ExternalTransactionId = string.IsNullOrWhiteSpace(request.ExternalTransactionId)
                            ? null
                            : request.ExternalTransactionId
                    });

                var result = await _mediator.Send(command, context.CancellationToken);

                if (!result.IsSuccess)
                {
                    if (result.Status == ResultStatus.Invalid)
                    {
                        var message = string.Join(", ",
                            result.ValidationErrors.Select(e => e.ErrorMessage));

                        throw new RpcException(new Status(
                            StatusCode.InvalidArgument,
                            message));
                    }

                    throw new RpcException(new Status(
                        StatusCode.FailedPrecondition,
                        string.Join(", ", result.Errors)));
                }

                var data = result.Value;

                return new AddBalanceResponse
                {
                    WalletId = data.WalletId.ToString(),
                    UserId = data.UserId.ToString(),
                    BalanceBefore = data.BalanceBefore.ToString("F2"),
                    BalanceAfter = data.BalanceAfter.ToString("F2"),
                    AmountAdded = data.AmountAdded.ToString("F2"),
                    TransactionId = data.TransactionId.ToString(),
                    UpdatedAt = data.UpdatedAt.ToString("O")
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Concurrency conflict on attempt {Attempt}/{MaxRetries} for user {UserId}",
                    attempt, maxRetries, userId);

                if (attempt == maxRetries)
                    throw new RpcException(new Status(
                        StatusCode.Aborted,
                        "Concurrency conflict: Another operation modified the wallet. Please retry."));

                // Brief delay before retry
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), context.CancellationToken);
            }
        }

        // Should never reach here
        throw new RpcException(new Status(
            StatusCode.Internal,
            "Unexpected error in AddBalance"));
    }

    public override async Task<DeductBalanceResponse> DeductBalance(
        DeductBalanceRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid user_id format"));

        if (!decimal.TryParse(request.Amount, out var amount) || amount <= 0)
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid amount. Must be a positive number"));

        // ✅ Retry loop for optimistic concurrency
        const int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var walletQuery = new GetWalletByUserIdQuery(userId);
                var walletResult = await _mediator.Send(walletQuery, context.CancellationToken);

                if (walletResult.Status == Ardalis.Result.ResultStatus.NotFound)
                    throw new RpcException(new Status(
                        StatusCode.NotFound,
                        "Wallet not found for user"));

                var wallet = walletResult.Value;

                var command = new DeductBalanceCommand(
                    wallet.Id,
                    userId,
                    new DeductBalanceCommand.DeductBalanceCommandRequest
                    {
                        Amount = amount,
                        RowVersion = wallet.RowVersion,
                        Description = request.Description,
                        PaymentMethod = MapGrpcPaymentMethodToDomain(request.PaymentMethod),
                        ExternalTransactionId = string.IsNullOrWhiteSpace(request.ExternalTransactionId)
                            ? null
                            : request.ExternalTransactionId
                    });

                var result = await _mediator.Send(command, context.CancellationToken);

                if (!result.IsSuccess)
                {
                    if (result.Status == ResultStatus.Invalid)
                    {
                        var message = string.Join(", ",
                            result.ValidationErrors.Select(e => e.ErrorMessage));

                        throw new RpcException(new Status(
                            StatusCode.InvalidArgument,
                            message));
                    }

                    throw new RpcException(new Status(
                        StatusCode.FailedPrecondition,
                        string.Join(", ", result.Errors)));
                }

                var data = result.Value;

                return new DeductBalanceResponse
                {
                    WalletId = data.WalletId.ToString(),
                    UserId = data.UserId.ToString(),
                    BalanceBefore = data.BalanceBefore.ToString("F2"),
                    BalanceAfter = data.BalanceAfter.ToString("F2"),
                    AmountDeducted = data.AmountDeducted.ToString("F2"),
                    TransactionId = data.TransactionId.ToString(),
                    UpdatedAt = data.UpdatedAt.ToString("O")
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Concurrency conflict on attempt {Attempt}/{MaxRetries} for user {UserId}",
                    attempt, maxRetries, userId);

                if (attempt == maxRetries)
                    throw new RpcException(new Status(
                        StatusCode.Aborted,
                        "Concurrency conflict: Another operation modified the wallet. Please retry."));

                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), context.CancellationToken);
            }
        }

        throw new RpcException(new Status(
            StatusCode.Internal,
            "Unexpected error in DeductBalance"));
    }

    public override async Task<UpdateWalletStatusResponse> UpdateWalletStatus(
        UpdateWalletStatusRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.WalletId, out var walletId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid wallet_id format"));

        if (!Guid.TryParse(request.AdminId, out var adminId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid admin_id format"));

        var command = new UpdateWalletStatusCommand(
            walletId,
            adminId,
            new UpdateWalletStatusCommand.UpdateWalletStatusRequest
            {
                NewStatus = MapGrpcStatusToDomain((Grpc.WalletStatus)request.NewStatus),
                Reason = request.Reason,
                AdminNotes = request.AdminNotes
            });

        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                string.Join(", ", result.Errors)));

        var data = result.Value;

        return new UpdateWalletStatusResponse
        {
            WalletId = data.WalletId.ToString(),
            OldStatus = MapDomainStatusToGrpc(data.OldStatus),
            NewStatus = MapDomainStatusToGrpc(data.NewStatus),
            Reason = data.Reason ?? "",
            UpdatedBy = data.UpdatedBy.ToString(),
            UpdatedAt = data.UpdatedAt.ToString("O")
        };
    }

    public override async Task<GetWalletByUserIdResponse> GetWalletByUserId(
        GetWalletByUserIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid user_id format"));

        var result = await _mediator.Send(
            new GetWalletByUserIdQuery(userId),
            context.CancellationToken);

        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            throw new RpcException(new Status(
                StatusCode.NotFound,
                result.Errors.FirstOrDefault() ?? "Wallet not found"));

        var wallet = result.Value;

        return new GetWalletByUserIdResponse
        {
            Id = wallet.Id.ToString(),
            UserId = wallet.UserId.ToString(),
            Balance = wallet.Balance.ToString("F2"),
            Status = (Grpc.WalletStatus)wallet.Status,
            CreatedAt = wallet.CreatedAt.ToString("O"),
            UpdatedAt = wallet.UpdatedAt.ToString("O")
        };
    }

    // ========== MAPPING HELPERS ==========

    private static WalletStatus MapDomainStatusToGrpc(Domain.Enums.WalletStatus domainStatus)
    {
        return domainStatus switch
        {
            Domain.Enums.WalletStatus.Inactive => WalletStatus.Inactive,
            Domain.Enums.WalletStatus.Active => WalletStatus.Active,
            Domain.Enums.WalletStatus.Locked => WalletStatus.Locked,
            Domain.Enums.WalletStatus.Suspended => WalletStatus.Suspended,
            _ => WalletStatus.Unspecified
        };
    }

    private static Domain.Enums.WalletStatus MapGrpcStatusToDomain(Grpc.WalletStatus grpcStatus)
    {
        return grpcStatus switch
        {
            Grpc.WalletStatus.Inactive => Domain.Enums.WalletStatus.Inactive,
            Grpc.WalletStatus.Active => Domain.Enums.WalletStatus.Active,
            Grpc.WalletStatus.Locked => Domain.Enums.WalletStatus.Locked,
            Grpc.WalletStatus.Suspended => Domain.Enums.WalletStatus.Suspended,
            _ => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid wallet status"))
        };
    }

    private static Domain.Enums.PaymentMethod? MapGrpcPaymentMethodToDomain(
        WalletPaymentMethod grpcMethod)
    {
        return grpcMethod switch
        {
            WalletPaymentMethod.PaymentMethodCash => Domain.Enums.PaymentMethod.Cash,
            WalletPaymentMethod.PaymentMethodVnpay => Domain.Enums.PaymentMethod.VNPay,
            WalletPaymentMethod.PaymentMethodWallet => Domain.Enums.PaymentMethod.Wallet,
            WalletPaymentMethod.PaymentMethodUnspecified => null,
            _ => null
        };
    }
}

