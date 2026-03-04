using AutoMapper;
using Grpc.Core;
using MediatR;
using SalesService.Application.Features.Payments.CreatePayment;
using SalesService.Application.Features.Payments.GetAll;
using SalesService.Application.Features.Payments.GetMyPaymentHistory;
<<<<<<< HEAD
=======
using SalesService.Application.Features.Payments.VnPayReturn;
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
using SalesService.Domain.Enums;

namespace SalesService.API.Services.Payments;

public class PaymentGrpcService : PaymentGrpc.PaymentGrpcBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public PaymentGrpcService(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    public override async Task<PaymentResponse> CreatePayment(CreatePaymentRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid order id"));
        }

        // Get returnUrl from request (optional, can be empty)
        var returnUrl = string.IsNullOrWhiteSpace(request.ReturnUrl) ? null : request.ReturnUrl;

        var command = new CreatePaymentCommand
            (
                new CreatePaymentReq
                {
                    OrderId = orderId,
                    ReturnUrl = returnUrl,
                    PaymentMethod = (PaymentMethod)request.PaymentMethod,
                }
            );
        var result = await _mediator.Send(command, context.CancellationToken);

        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
        {
            throw new RpcException(
                new Status(StatusCode.NotFound, result.Errors.FirstOrDefault() ?? "Order not found"));
        }

        if (!result.IsSuccess)
        {
            throw new RpcException(
                new Status(StatusCode.Internal, result.Errors.FirstOrDefault() ?? "Failed to create VNPay payment url"));
        }

        return new PaymentResponse
        {
            Url = result.Value.Url
        };
    }

    public override async Task<GetAllPaymentsReply> GetAllPayments(
        GetAllPaymentsRequest request,
        ServerCallContext context)
    {
        var query = new GetAllPaymentsQuery
        {
            Page = request.Page <= 0 ? 1 : request.Page,
            PageSize = request.PageSize <= 0 ? 10 : request.PageSize
        };

        var paged = await _mediator.Send(query, context.CancellationToken);

        var reply = new GetAllPaymentsReply
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages
        };

        reply.Payments.AddRange(paged.Items.Select(p => new PaymentDto
        {
            Id = p.Id.ToString(),
            OrderId = p.OrderId.ToString(),
            Code = p.Code,
            Amount = (double)p.Amount,
            Currency = p.Currency,
            Method = p.Method,
            Status = p.Status,
            PaidAt = p.PaidAt?.ToString("O") ?? string.Empty,
            CreatedAt = p.CreatedAt?.ToString("O") ?? string.Empty
        }));

        return reply;
    }

    public override async Task<GetMyPaymentHistoryReply> GetMyPaymentHistory(
        GetMyPaymentHistoryRequest request,
        ServerCallContext context)
    {
        // Lấy customerId từ metadata header x-user-id
        var userIdEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));

        if (userIdEntry == null)
        {
            throw new RpcException(
                new Status(StatusCode.Unauthenticated, "Missing x-user-id in request headers"));
        }

        if (!Guid.TryParse(userIdEntry.Value, out var customerId))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid x-user-id format"));
        }

        DateTime? startDate = null;
        DateTime? endDate = null;

        if (!string.IsNullOrWhiteSpace(request.StartDate))
        {
            if (!DateTime.TryParse(request.StartDate, out var sd))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid start_date format. Use ISO-8601, e.g. 2025-01-01"));
            startDate = DateTime.SpecifyKind(sd.Date, DateTimeKind.Utc);
        }

        if (!string.IsNullOrWhiteSpace(request.EndDate))
        {
            if (!DateTime.TryParse(request.EndDate, out var ed))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid end_date format. Use ISO-8601, e.g. 2025-12-31"));
            // include the entire end day
            endDate = DateTime.SpecifyKind(ed.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
        }

        int? status = request.HasStatus ? request.Status : null;

        var query = new GetMyPaymentHistoryQuery
        {
            CustomerId = customerId,
            Page = request.Page <= 0 ? 1 : request.Page,
            PageSize = request.PageSize <= 0 ? 10 : request.PageSize,
            StartDate = startDate,
            EndDate = endDate,
            Status = status
        };

        var paged = await _mediator.Send(query, context.CancellationToken);

        var reply = new GetMyPaymentHistoryReply
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages
        };

        reply.Items.AddRange(paged.Items.Select(p => new PaymentHistoryItemDto
        {
            Id = p.Id.ToString(),
            OrderId = p.OrderId.ToString(),
            OrderCode = p.OrderCode,
            Amount = (double)p.Amount,
            Currency = p.Currency,
            PaymentMethod = p.PaymentMethod,
            Status = p.Status,
            PaymentDate = p.PaymentDate?.ToString("O") ?? string.Empty
        }));

        return reply;
    }
<<<<<<< HEAD
}
=======

    public override async Task<VnPayReturnResponse> ProcessVnPayReturn(VnPayReturnRequest request, ServerCallContext context)
    {
        var queryDict = request.QueryParams.ToDictionary(k => k.Key, v => v.Value);

        var cmd = new VnPayReturnCommand(queryDict);
        var result = await _mediator.Send(cmd);

        if (result.IsSuccess && result.Value != null)
        {
            return new VnPayReturnResponse
            {
                IsSuccess = result.Value.Success,
                Message = "Processed successfully"
            };
        }

        return new VnPayReturnResponse
        {
            IsSuccess = false,
            Message = string.Join(", ", result.Errors)
        };
    }
}
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
