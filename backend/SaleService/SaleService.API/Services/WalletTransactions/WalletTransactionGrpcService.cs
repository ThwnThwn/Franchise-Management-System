using Ardalis.Result;
using Grpc.Core;
using MediatR;
using SalesService.Application.Features.WalletTransactions.Commands.CreateTransaction;
using SalesService.Application.Features.WalletTransactions.Queries.GetAll;
using SalesService.Application.Features.WalletTransactions.Queries.GetByCode;
using SalesService.Application.Features.WalletTransactions.Queries.GetById;
using SalesService.Application.Features.WalletTransactions.Queries.GetByUserId;
using SalesService.Application.Features.WalletTransactions.Querries.GetByOrderId;
using SalesService.Grpc;
using DomainPaymentMethod = SalesService.Domain.Enums.PaymentMethod;
using DomainReferenceType = SalesService.Domain.Enums.WalletReferenceType;
using DomainTransactionStatus = SalesService.Domain.Enums.TransactionStatus;
using DomainTransactionType = SalesService.Domain.Enums.TransactionType;

namespace SalesService.API.GrpcServices;

public sealed class WalletTransactionGrpcService : WalletTransactionGrpc.WalletTransactionGrpcBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WalletTransactionGrpcService> _logger;

    public WalletTransactionGrpcService(
        IMediator mediator,
        ILogger<WalletTransactionGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<Grpc.CreateTransactionResponse> CreateTransaction(
        Grpc.CreateTransactionRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid user_id format"));

        if (!Guid.TryParse(request.WalletId, out var walletId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid wallet_id format"));

        if (!decimal.TryParse(request.Amount, out var amount) || amount <= 0)
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid amount. Must be positive"));

        Guid? referenceId = null;
        if (!string.IsNullOrWhiteSpace(request.ReferenceId))
        {
            if (!Guid.TryParse(request.ReferenceId, out var parsedRefId))
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    "Invalid reference_id format"));
            referenceId = parsedRefId;
        }

        Guid? destinationWalletId = null;
        if (!string.IsNullOrWhiteSpace(request.DestinationWalletId))
        {
            if (!Guid.TryParse(request.DestinationWalletId, out var parsedDestId))
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    "Invalid destination_wallet_id format"));
            destinationWalletId = parsedDestId;
        }

        Guid? adminId = null;
        if (request.AdminOverride != null)
        {
            if (!Guid.TryParse(request.AdminOverride.AdminId, out var parsedAdminId))
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    "Invalid admin_id format"));
            adminId = parsedAdminId;
        }

        var innerRequest = new CreateTransactionCommand.CreateTransactionRequest
        {
            WalletId = walletId,
            Amount = amount,
            TransactionType = MapGrpcTransactionTypeToDomain(request.TransactionType),
            PaymentMethod = MapGrpcPaymentMethodToDomain(request.PaymentMethod),
            ReferenceId = referenceId,
            ReferenceType = MapGrpcReferenceTypeToDomain(request.ReferenceType),
            Description = request.Description,
            Notes = request.Notes,
            ExternalTransactionId = request.ExternalTransactionId,
            DestinationWalletId = destinationWalletId,
            Metadata = request.Metadata
        };

        var command = new CreateTransactionCommand(
            userId,
            innerRequest,
            request.AdminOverride != null
                ? new CreateTransactionCommand.HasAdminOverride
                {
                    AdminId = adminId!.Value,
                    AutoComplete = request.AdminOverride.AutoComplete,
                    BypassBalanceCheck = request.AdminOverride.BypassBalanceCheck,
                    BypassTransactionLimits = request.AdminOverride.BypassTransactionLimits,
                    InternalNotes = request.AdminOverride.InternalNotes
                }
                : null
        );

        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == ResultStatus.Invalid)
            {
                var message = string.Join(" | ",
                    result.ValidationErrors.Select(e =>
                        $"{e.Identifier}: {e.ErrorMessage}"));

                _logger.LogWarning("Validation failed: {Message}", message);

                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    message));
            }

            var errorMessage = string.Join(" | ", result.Errors);

            _logger.LogError("Business error: {Error}", errorMessage);

            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                errorMessage));
        }

        var data = result.Value;

        return new Grpc.CreateTransactionResponse
        {
            Id = data.Id.ToString(),
            TransactionCode = data.TransactionCode,
            WalletId = data.WalletId.ToString(),
            Amount = data.Amount.ToString("F2"),
            TransactionType = MapDomainTransactionTypeToGrpc(data.TransactionType),
            Status = MapDomainTransactionStatusToGrpc(data.Status),
            BalanceBefore = data.BalanceBefore.ToString("F2"),
            BalanceAfter = data.BalanceAfter.ToString("F2"),
            ReferenceId = data.ReferenceId?.ToString() ?? "",
            ReferenceType = MapDomainReferenceTypeToGrpc(data.ReferenceType),
            PaymentMethod = MapDomainPaymentMethodToGrpc(data.PaymentMethod),
            CreatedAt = data.CreatedAt.ToString("O")
        };
    }

    public override async Task<GetAllWalletTransactionsResponse> GetAll(
        GetAllWalletTransactionsRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.LoggedUserId, out var loggedUserId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid logged_user_id format"));

        var pageIndex = request.PageIndex == 0 ? 1 : request.PageIndex;
        var pageSize = request.PageSize == 0 ? 20 : request.PageSize;
        var query = new GetAllWalletTransactionsQuery(loggedUserId, pageIndex, pageSize);

        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == ResultStatus.Invalid)
            {
                var message = string.Join(" | ",
                    result.ValidationErrors.Select(e =>
                        $"{e.Identifier}: {e.ErrorMessage}"));

                _logger.LogWarning("Validation failed: {Message}", message);

                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    message));
            }

            var errorMessage = string.Join(" | ", result.Errors);

            _logger.LogError("Business error: {Error}", errorMessage);

            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                errorMessage));
        }

        var response = new GetAllWalletTransactionsResponse
        {
            Meta = new WalletTransactionPagedResponseMeta
            {
                PageIndex = result.Value.Page,
                PageSize = result.Value.PageSize,
                TotalCount = (int)result.Value.TotalCount,
                TotalPages = result.Value.TotalPages
            }
        };

        response.Transactions.AddRange(
            result.Value.Items.Select(MapToGrpcTransactionResponse));

        return response;
    }

    public override async Task<Grpc.WalletTransactionResponse> GetById(
        GetWalletTransactionByIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.TransactionId, out var transactionId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid transaction_id format"));

        if (!Guid.TryParse(request.LoggedUserId, out var loggedUserId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid logged_user_id format"));

        Guid TransactionId = transactionId;
        Guid LoggedUserId = loggedUserId;

        var query = new GetWalletTransactionByIdQuery(TransactionId, LoggedUserId);

        var result = await _mediator.Send(query, context.CancellationToken);

        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            throw new RpcException(new Status(
                StatusCode.NotFound,
                result.Errors.FirstOrDefault() ?? "Transaction not found"));

        if (!result.IsSuccess)
        {
            if (result.Status == ResultStatus.Invalid)
            {
                var message = string.Join(" | ",
                    result.ValidationErrors.Select(e =>
                        $"{e.Identifier}: {e.ErrorMessage}"));

                _logger.LogWarning("Validation failed: {Message}", message);

                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    message));
            }

            var errorMessage = string.Join(" | ", result.Errors);

            _logger.LogError("Business error: {Error}", errorMessage);

            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                errorMessage));
        }

        return MapToGrpcTransactionResponse(result.Value);
    }

    public override async Task<GetAllWalletTransactionsResponse> GetByUserId(
        GetWalletTransactionsByUserIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserIdToSearch, out var userIdToSearch))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid user_id_to_search format"));

        if (!Guid.TryParse(request.LoggedUserId, out var loggedUserId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid logged_user_id format"));

        var query = new GetWalletTransactionsByUserIdQuery
            (userIdToSearch, loggedUserId, request.PageIndex == 0 ? 1 : request.PageIndex, request.PageSize == 0 ? 20 : request.PageSize
        );

        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == ResultStatus.Invalid)
            {
                var message = string.Join(" | ",
                    result.ValidationErrors.Select(e =>
                        $"{e.Identifier}: {e.ErrorMessage}"));

                _logger.LogWarning("Validation failed: {Message}", message);

                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    message));
            }

            var errorMessage = string.Join(" | ", result.Errors);

            _logger.LogError("Business error: {Error}", errorMessage);

            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                errorMessage));
        }

        var response = new GetAllWalletTransactionsResponse
        {
            Meta = new WalletTransactionPagedResponseMeta
            {
                PageIndex = result.Value.Page,
                PageSize = result.Value.PageSize,
                TotalCount = (int)result.Value.TotalCount,
                TotalPages = result.Value.TotalPages
            }
        };

        response.Transactions.AddRange(
            result.Value.Items.Select(MapToGrpcTransactionResponse));

        return response;
    }

    public override async Task<Grpc.WalletTransactionResponse> GetByCode(
        GetWalletTransactionByCodeRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionCode))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Transaction code is required"));

        if (!Guid.TryParse(request.LoggedUserId, out var loggedUserId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid logged_user_id format"));

        var query = new GetWalletTransactionByCodeQuery(request.TransactionCode, loggedUserId);

        var result = await _mediator.Send(query, context.CancellationToken);

        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            throw new RpcException(new Status(
                StatusCode.NotFound,
                result.Errors.FirstOrDefault() ?? "Transaction not found"));

        if (!result.IsSuccess)
        {
            if (result.Status == ResultStatus.Invalid)
            {
                var message = string.Join(" | ",
                    result.ValidationErrors.Select(e =>
                        $"{e.Identifier}: {e.ErrorMessage}"));

                _logger.LogWarning("Validation failed: {Message}", message);

                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    message));
            }

            var errorMessage = string.Join(" | ", result.Errors);

            _logger.LogError("Business error: {Error}", errorMessage);

            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                errorMessage));
        }

        return MapToGrpcTransactionResponse(result.Value);
    }

    public override async Task<GetAllWalletTransactionsResponse> GetByOrderId(
        GetWalletTransactionByOrderIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid order_id format"));

        if (!Guid.TryParse(request.LoggedUserId, out var loggedUserId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid logged_user_id format"));

        var query = new GetWalletTransactionByOrderIdQuery(orderId, loggedUserId);

        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == ResultStatus.Invalid)
            {
                var message = string.Join(" | ",
                    result.ValidationErrors.Select(e =>
                        $"{e.Identifier}: {e.ErrorMessage}"));

                _logger.LogWarning("Validation failed: {Message}", message);

                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    message));
            }

            var errorMessage = string.Join(" | ", result.Errors);

            _logger.LogError("Business error: {Error}", errorMessage);

            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                errorMessage));
        }

        var response = new GetAllWalletTransactionsResponse
        {
            Meta = new WalletTransactionPagedResponseMeta
            {
                PageIndex = 1,
                PageSize = result.Value.Count,
                TotalCount = result.Value.Count,
                TotalPages = 1
            }
        };

        response.Transactions.AddRange(
            result.Value.Select(MapToGrpcTransactionResponse));

        return response;
    }

    // ========== MAPPING HELPERS ==========

    private static Grpc.WalletTransactionResponse MapToGrpcTransactionResponse(
        dynamic transaction)
    {
        return new Grpc.WalletTransactionResponse
        {
            Id = transaction.Id.ToString(),
            WalletId = transaction.WalletId.ToString(),
            UserId = transaction.UserId.ToString(),
            OrderId = transaction.OrderId?.ToString() ?? "",
            InitiatedBy = transaction.InitiatedBy?.ToString() ?? "",
            ProcessedBy = transaction.ProcessedBy?.ToString() ?? "",
            ParentTransactionId = transaction.ParentTransactionId?.ToString() ?? "",
            TransactionCode = transaction.TransactionCode ?? "",
            Notes = transaction.Notes ?? "",
            Amount = transaction.Amount.ToString("F2"),
            BalanceBefore = transaction.BalanceBefore.ToString("F2"),
            BalanceAfter = transaction.BalanceAfter.ToString("F2"),
            TransactionType = MapDomainTransactionTypeToGrpc(transaction.TransactionType),
            Status = MapDomainTransactionStatusToGrpc(transaction.Status),
            ReferenceId = transaction.ReferenceId?.ToString() ?? "",
            ReferenceType = MapDomainReferenceTypeToGrpc(transaction.ReferenceType),
            Description = transaction.Description ?? "",
            ExternalTransactionId = transaction.ExternalTransactionId ?? "",
            Metadata = transaction.Metadata ?? "",
            PaymentMethod = MapDomainPaymentMethodToGrpc(transaction.PaymentMethod),
            CreatedAt = transaction.CreatedAt.ToString("O"),
            UpdatedAt = transaction.UpdatedAt.ToString("O"),
            CompletedAt = transaction.CompletedAt?.ToString("O") ?? "",
            FailedAt = transaction.FailedAt?.ToString("O") ?? "",
            ProcessedAt = transaction.ProcessedAt?.ToString("O") ?? "",
            FailureReason = transaction.FailureReason ?? ""
        };
    }

    private static Grpc.TransactionType MapDomainTransactionTypeToGrpc(
        DomainTransactionType domainType) => domainType switch
        {
            DomainTransactionType.Deposit => Grpc.TransactionType.Deposit,
            DomainTransactionType.Payment => Grpc.TransactionType.Payment,
            DomainTransactionType.Refund => Grpc.TransactionType.Refund,
            DomainTransactionType.Adjustment => Grpc.TransactionType.Adjustment,
            DomainTransactionType.Withdrawal => Grpc.TransactionType.Withdrawal,
            DomainTransactionType.Transfer => Grpc.TransactionType.Transfer,
            _ => Grpc.TransactionType.Unspecified
        };

    private static DomainTransactionType MapGrpcTransactionTypeToDomain(
        Grpc.TransactionType grpcType) => grpcType switch
        {
            Grpc.TransactionType.Deposit => DomainTransactionType.Deposit,
            Grpc.TransactionType.Payment => DomainTransactionType.Payment,
            Grpc.TransactionType.Refund => DomainTransactionType.Refund,
            Grpc.TransactionType.Adjustment => DomainTransactionType.Adjustment,
            Grpc.TransactionType.Withdrawal => DomainTransactionType.Withdrawal,
            Grpc.TransactionType.Transfer => DomainTransactionType.Transfer,
            _ => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid transaction type"))
        };

    private static Grpc.TransactionStatus MapDomainTransactionStatusToGrpc(
        DomainTransactionStatus domainStatus) => domainStatus switch
        {
            DomainTransactionStatus.Pending => Grpc.TransactionStatus.Pending,
            DomainTransactionStatus.Completed => Grpc.TransactionStatus.Completed,
            DomainTransactionStatus.Failed => Grpc.TransactionStatus.Failed,
            DomainTransactionStatus.Cancelled => Grpc.TransactionStatus.Cancelled,
            _ => Grpc.TransactionStatus.Unspecified
        };

    private static Grpc.WalletReferenceType MapDomainReferenceTypeToGrpc(
        DomainReferenceType? domainType) => domainType switch
        {
            DomainReferenceType.Order => Grpc.WalletReferenceType.ReferenceTypeOrder,
            DomainReferenceType.RefundTicket => Grpc.WalletReferenceType.ReferenceTypeRefundTicket,
            DomainReferenceType.InternalUser => Grpc.WalletReferenceType.ReferenceTypeInternalUser,
            DomainReferenceType.System => Grpc.WalletReferenceType.ReferenceTypeSystem,
            _ => Grpc.WalletReferenceType.ReferenceTypeUnspecified
        };

    private static DomainReferenceType? MapGrpcReferenceTypeToDomain(
        Grpc.WalletReferenceType grpcType) => grpcType switch
        {
            Grpc.WalletReferenceType.ReferenceTypeOrder => DomainReferenceType.Order,
            Grpc.WalletReferenceType.ReferenceTypeRefundTicket => DomainReferenceType.RefundTicket,
            Grpc.WalletReferenceType.ReferenceTypeInternalUser => DomainReferenceType.InternalUser,
            Grpc.WalletReferenceType.ReferenceTypeSystem => DomainReferenceType.System,
            Grpc.WalletReferenceType.ReferenceTypeUnspecified => null,
            _ => null
        };

    private static WalletTransactionPaymentMethod MapDomainPaymentMethodToGrpc(
        DomainPaymentMethod? domainMethod) => domainMethod switch
        {
            DomainPaymentMethod.Cash => WalletTransactionPaymentMethod.PaymentMethodCash,
            DomainPaymentMethod.VNPay => WalletTransactionPaymentMethod.PaymentMethodVnpay,
            DomainPaymentMethod.Wallet => WalletTransactionPaymentMethod.PaymentMethodWallet,
            _ => WalletTransactionPaymentMethod.PaymentMethodUnspecified
        };

    private static DomainPaymentMethod? MapGrpcPaymentMethodToDomain(
        WalletTransactionPaymentMethod grpcMethod) => grpcMethod switch
        {
            WalletTransactionPaymentMethod.PaymentMethodCash => DomainPaymentMethod.Cash,
            WalletTransactionPaymentMethod.PaymentMethodVnpay => DomainPaymentMethod.VNPay,
            WalletTransactionPaymentMethod.PaymentMethodWallet => DomainPaymentMethod.Wallet,
            WalletTransactionPaymentMethod.PaymentMethodUnspecified => null,
            _ => null
        };
}