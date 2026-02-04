using AutoMapper;
using Grpc.Core;
using MediatR;
using SalesService.Application.Features.Payments.CreatePayment;
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
            Message = result.Value.Message
        };
    }
}
