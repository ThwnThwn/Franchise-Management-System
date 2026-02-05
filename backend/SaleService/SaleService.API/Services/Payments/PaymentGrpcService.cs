using AutoMapper;
using Grpc.Core;
using MediatR;
using SalesService.Application.Features.Payments.CreatePayment;
using SalesService.Application.Features.Payments.GetAll;
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
}
