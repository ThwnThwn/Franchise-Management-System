using System;
using Ardalis.Result;
using MediatR;

namespace SalesService.Application.Features.Payments.CreateVnPayUrl;

public sealed record CreateVnPayUrlCommand(Guid OrderId)
    : IRequest<Result<CreateVnPayUrlResponse>>;

public sealed class CreateVnPayUrlResponse
{
    public string PaymentUrl { get; set; } = default!;
}

