using Ardalis.Result;
using Grpc.Core;
using MediatR;
using SalesService.Application.Features.RefundTickets.Command.ApproveRefund;
using SalesService.Application.Features.RefundTickets.Command.CancelRefund;
using SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer;
using SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund;
using SalesService.Application.Features.RefundTickets.Command.RejectRefundByAdmin;
using SalesService.Application.Features.RefundTickets.Queries.GetAll;
using SalesService.Application.Features.RefundTickets.Queries.GetById;
using SalesService.Application.Features.RefundTickets.Queries.GetByStatus;
using SalesService.Application.Features.RefundTickets.Queries.GetByUserId;
using SalesService.Grpc;
using static SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund.InternalUserCreateRefundTicketCommand;
using DomainRefundMethod = SalesService.Domain.Enums.RefundTicketMethod;
using DomainRefundReason = SalesService.Domain.Enums.RefundReasonType;
using DomainRefundStatus = SalesService.Domain.Enums.RefundTicketStatus;

namespace SalesService.API.GrpcServices;

public sealed class RefundGrpcService : RefundTicketGrpcService.RefundTicketGrpcServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RefundGrpcService> _logger;

    public RefundGrpcService(
        IMediator mediator,
        ILogger<RefundGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<CustomerCreateRefundResponse> CustomerCreateRefund(
        CustomerCreateRefundRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.CustomerId, out var customerId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid customer_id format"));

        if (!Guid.TryParse(request.OrderId, out var orderId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid order_id format"));

        if (!decimal.TryParse(request.Amount, out var amount) || amount <= 0)
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid amount. Must be a positive number"));
        var commandRequest = new CustomerCreateRefundTicketCommand.CustomerCreateRefundTicketRequest
        {
            Amount = amount,
            Reason = request.Reason
        };
        var command = new CustomerCreateRefundTicketCommand(
        customerId,
        orderId,
        commandRequest);

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

        return new CustomerCreateRefundResponse
        {
            Id = data.Id.ToString(),
            TicketCode = data.TicketCode,
            Amount = data.Amount.ToString("F2"),
            Reason = data.Reason ?? "",
            OrderId = data.OrderId.ToString(),
            Status = MapDomainStatusToGrpc(data.Status),
            CreatedAt = data.CreatedAt.ToString("O")
        };
    }

    public override async Task<InternalCreateRefundResponse> InternalCreateRefund(
        InternalCreateRefundRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid user_id format"));

        if (!Guid.TryParse(request.OriginalTransactionId, out var originalTransactionId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid original_transaction_id format"));

        if (!decimal.TryParse(request.Amount, out var amount) || amount <= 0)
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid amount. Must be a positive number"));


        var commandRequest = new InternalUserCreateRefundTicketRequest
        {
            WalletId = Guid.Parse(request.WalletId),
            OriginalTransactionId = Guid.Parse(request.OriginalTransactionId),
            Amount = decimal.Parse(request.Amount),
            Reason = request.Reason,
            Notes = request.InternalNotes
        };

        var overrideInfo = new Application.Common.Models.InternalUserOverrideInfo
        {
            AutoApprove = request.OverrideInfo.AutoApprove,
            BypassAmountValidation = request.OverrideInfo.BypassAmountValidation,
            BypassTimeLimit = request.OverrideInfo.BypassTimeLimit,
            RefundReason = (Domain.Enums.RefundReasonType)request.OverrideInfo.RefundReason
        };

        var command = new InternalUserCreateRefundTicketCommand
        (
            userId,
            commandRequest,
            overrideInfo
        );

        var result = await _mediator.Send(command, context.CancellationToken);

        if(!result.IsSuccess)
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

        return new InternalCreateRefundResponse
        {
            Id = data.Id.ToString(),
            TicketCode = data.TicketCode,
            Amount = data.Amount.ToString("F2"),
            Reason = data.Reason ?? "",
            WalletId = data.WalletId.ToString(),
            OrderId = data.OrderId?.ToString() ?? "",
            TransactionId = data.TransactionId?.ToString() ?? "",
            OriginalTransactionId = data.OriginalTransactionId.ToString(),
            Status = MapDomainStatusToGrpc(data.Status),
            CreatedAt = data.CreatedAt.ToString("O")
        };
    }

    public override async Task<ApproveRefundResponse> ApproveRefund(
        ApproveRefundRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.RefundTicketId, out var refundTicketId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid refund_ticket_id format"));

        if (!Guid.TryParse(request.InternalUserId, out var internalUserId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid internal_user_id format"));

        var requestData = new ApproveRefundCommand.ApproveRefundRequest
        {
            ApprovalNotes = request.ApprovalNotes,
            AutoComplete = request.AutoComplete
        };

        var command = new ApproveRefundCommand
        (refundTicketId, internalUserId, requestData);

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

        return new ApproveRefundResponse
        {
            RefundTicketId = data.RefundTicketId.ToString(),
            TicketCode = data.TicketCode,
            Status = MapDomainStatusToGrpc(data.Status),
            Amount = data.Amount.ToString("F2"),
            CustomerId = data.CustomerId.ToString(),
            ApprovedBy = data.ApprovedBy.ToString(),
            ApprovedAt = data.ApprovedAt.ToString("O"),
            ApprovalNotes = data.ApprovalNotes ?? "",
            NextSteps = data.NextSteps ?? ""
        };
    }

    public override async Task<RejectRefundResponse> RejectRefund(
        RejectRefundRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.RefundTicketId, out var refundTicketId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid refund_ticket_id format"));

        if (!Guid.TryParse(request.InternalUserId, out var internalUserId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid internal_user_id format"));

        var requestData = new RejectRefundCommand.RejectRefundRequest
        {
            RejectNotes = request.RejectNotes
        };

        var command = new RejectRefundCommand
       (refundTicketId, internalUserId, requestData);

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

        return new RejectRefundResponse
        {
            RefundTicketId = data.RefundTicketId.ToString(),
            TicketCode = data.TicketCode,
            Status = MapDomainStatusToGrpc(data.Status),
            Amount = data.Amount.ToString("F2"),
            CustomerId = data.CustomerId.ToString(),
            RejectedBy = data.RejectdBy.ToString(),
            RejectedAt = data.RejectdAt.ToString("O"),
            RejectNotes = data.RejectNotes ?? "",
            NextSteps = data.NextSteps ?? ""
        };
    }

    public override async Task<CancelRefundResponse> CancelRefund(
        CancelRefundRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.RefundTicketId, out var refundTicketId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid refund_ticket_id format"));

        if (!Guid.TryParse(request.CustomerId, out var customerId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid customer_id format"));
        var requestData = new CancelRefundCommand.CancelRefundRequest
        {
            CancellationReason = request.CancellationReason
        };
        var command = new CancelRefundCommand
        (refundTicketId, customerId, requestData);

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

        return new CancelRefundResponse
        {
            RefundTicketId = data.RefundTicketId.ToString(),
            TicketCode = data.TicketCode,
            Status = MapDomainStatusToGrpc(data.Status),
            Amount = data.Amount.ToString("F2"),
            CancellationReason = data.CancellationReason ?? "",
            CancelledAt = data.CancelledAt.ToString("O"),
            Message = data.Message ?? ""
        };
    }

    public override async Task<Grpc.GetRefundByIdResponse> GetRefundById(
        GetRefundByIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.RefundTicketId, out var refundTicketId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid refund_ticket_id format"));

        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid user_id format"));

        var query = new GetRefundByIdQuery(refundTicketId, userId);

        var result = await _mediator.Send(query, context.CancellationToken);

        if (result.Status == ResultStatus.NotFound)
            throw new RpcException(new Status(
                StatusCode.NotFound,
                result.Errors.FirstOrDefault() ?? "Refund ticket not found"));

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
        return new Grpc.GetRefundByIdResponse
        {
            Id = data.Id.ToString(),
            TicketCode = data.TicketCode,
            Amount = data.Amount.ToString("F2"),
            RefundAmount = data.Amount.ToString("F2"),
            Reason = data.Reason ?? "",
            RejectionReason = data.Reason ?? "",
            WalletId = data.WalletId.ToString(),
            UserId = data.UserId.ToString(),
            OrderId = data.OrderId?.ToString() ?? "",
            TransactionId = data.TransactionId?.ToString() ?? "",
            OriginalTransactionId = data.OriginalTransactionId.ToString() ?? "",
            Status = MapDomainStatusToGrpc(data.Status),
            RefundReason = MapDomainReasonTypeToGrpc(data.RefundReason),
            RefundMethod = MapDomainMethodToGrpc(data.RefundTicketMethod),
            CreatedAt = data.CreatedAt.ToString("O"),
            UpdatedAt = data.UpdatedAt.ToString("O")
        };
    }

    public override async Task<Grpc.GetAllRefundsResponse> GetAllRefunds(
        GetAllRefundsRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.InternalUserId, out var internalUserId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid internal_user_id format"));

        var query = new GetAllRefundsQuery
            (internalUserId,
            request.Paging?.PageIndex ?? 1,
            request.Paging?.PageSize ?? 20
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

        var response = new Grpc.GetAllRefundsResponse
        {
            Meta = new PagedResponseMeta
            {
                PageIndex = result.Value.Page,
                PageSize = result.Value.PageSize,
                TotalCount = (int)result.Value.TotalCount,
                TotalPages = result.Value.TotalPages
            }
        };

        response.Items.AddRange(
            result.Value.Items.Select(MapToRefundListItem));

        return response;
    }

    public override async Task<GetRefundByStatusResponse> GetRefundByStatus(
        GetRefundByStatusRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid user_id format"));

        var query = new GetRefundByStatusQuery
            (userId,
            MapGrpcStatusToDomain(request.Status),
            request.Paging?.PageIndex ?? 1,
            request.Paging?.PageSize ?? 20
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

        var response = new GetRefundByStatusResponse
        {
            Meta = new PagedResponseMeta
            {
                PageIndex = result.Value.Page,
                PageSize = result.Value.PageSize,
                TotalCount = (int)result.Value.TotalCount,
                TotalPages = result.Value.TotalPages
            }
        };

        response.Items.AddRange(
            result.Value.Items.Select(MapToRefundListItem));

        return response;
    }

    public override async Task<Grpc.GetRefundByUserIdResponse> GetRefundByUserId(
        GetRefundByUserIdRequest request,
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

        var query = new GetRefundByUserIdQuery
        (userIdToSearch,
          loggedUserId,
           request.Paging?.PageIndex ?? 1,
            request.Paging?.PageSize ?? 20
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

        var response = new Grpc.GetRefundByUserIdResponse
        {
            Meta = new PagedResponseMeta
            {
                PageIndex = result.Value.Page,
                PageSize = result.Value.PageSize,
                TotalCount = (int)result.Value.TotalCount,
                TotalPages = result.Value.TotalPages
            }
        };

        response.Items.AddRange(
            result.Value.Items.Select(MapToRefundListItem));

        return response;
    }

    // ========== MAPPING HELPERS ==========

    private static RefundListItem MapToRefundListItem(dynamic refund)
    {
        return new RefundListItem
        {
            Id = refund.Id.ToString(),
            TicketCode = refund.TicketCode,
            Amount = refund.Amount.ToString("F2"),
            RefundAmount = refund.RefundAmount.ToString("F2"),
            Reason = refund.Reason ?? "",
            WalletId = refund.WalletId.ToString(),
            UserId = refund.UserId.ToString(),
            OrderId = refund.OrderId?.ToString() ?? "",
            TransactionId = refund.TransactionId?.ToString() ?? "",
            OriginalTransactionId = refund.OriginalTransactionId?.ToString() ?? "",
            Status = MapDomainStatusToGrpc(refund.Status),
            CreatedAt = refund.CreatedAt.ToString("O"),
            UpdatedAt = refund.UpdatedAt.ToString("O")
        };
    }

    private static RefundTicketStatus MapDomainStatusToGrpc(
        DomainRefundStatus domainStatus) => domainStatus switch
        {
            DomainRefundStatus.Pending => RefundTicketStatus.RefundStatusPending,
            DomainRefundStatus.Approved => RefundTicketStatus.RefundStatusApproved,
            DomainRefundStatus.Processing => RefundTicketStatus.RefundStatusProcessing,
            DomainRefundStatus.Completed => RefundTicketStatus.RefundStatusCompleted,
            DomainRefundStatus.Failed => RefundTicketStatus.RefundStatusFailed,
            DomainRefundStatus.Rejected => RefundTicketStatus.RefundStatusRejected,
            DomainRefundStatus.Cancelled => RefundTicketStatus.RefundStatusCancelled,
            _ => RefundTicketStatus.RefundStatusUnspecified
        };

    private static DomainRefundStatus MapGrpcStatusToDomain(
        RefundTicketStatus grpcStatus) => grpcStatus switch
        {
            RefundTicketStatus.RefundStatusPending => DomainRefundStatus.Pending,
            RefundTicketStatus.RefundStatusApproved => DomainRefundStatus.Approved,
            RefundTicketStatus.RefundStatusProcessing => DomainRefundStatus.Processing,
            RefundTicketStatus.RefundStatusCompleted => DomainRefundStatus.Completed,
            RefundTicketStatus.RefundStatusFailed => DomainRefundStatus.Failed,
            RefundTicketStatus.RefundStatusRejected => DomainRefundStatus.Rejected,
            RefundTicketStatus.RefundStatusCancelled => DomainRefundStatus.Cancelled,
            _ => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid refund status"))
        };

    private static RefundReasonType MapDomainReasonTypeToGrpc(
        DomainRefundReason? domainReason) => domainReason switch
        {
            DomainRefundReason.NormalProcess => RefundReasonType.RefundReasonNormalProcess,
            DomainRefundReason.SystemError => RefundReasonType.RefundReasonSystemError,
            DomainRefundReason.CustomerComplaint => RefundReasonType.RefundReasonCustomerComplaint,
            DomainRefundReason.GoodwillGesture => RefundReasonType.RefundReasonGoodwillGesture,
            DomainRefundReason.Compensation => RefundReasonType.RefundReasonCompensation,
            _ => RefundReasonType.RefundReasonUnspecified
        };

    private static RefundTicketMethod MapDomainMethodToGrpc(
        DomainRefundMethod? domainMethod) => domainMethod switch
        {
            DomainRefundMethod.Transfer => RefundTicketMethod.RefundMethodTransfer,
            DomainRefundMethod.Cash => RefundTicketMethod.RefundMethodCash,
            _ => RefundTicketMethod.RefundMethodUnspecified
        };
}