using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using System;

namespace SalesService.Application.Features.Payments.CreatePayment
{
    public sealed record CreatePaymentCommand(CreatePaymentReq Requets)
        : IRequest<Result<CreatePaymentRes>>;

    public sealed class CreatePaymentRes
    {
        public string Url { get; set; } = default!;
    }

    public sealed class CreatePaymentReq
    {
        public Guid OrderId { get; set; }
        public string? ReturnUrl { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }

}
