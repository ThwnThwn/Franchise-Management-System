using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;

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
        public string? VoucherShipCode { get; set; }
        public string? VoucherDiscountCode { get; set; }
        public decimal? Wallet { get; set; }
        public int? Point { get; set; }
    }

}
