using FluentValidation;
using System;

namespace SalesService.Application.Features.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.FromDate.HasValue)
            .WithMessage("Ngày bắt đầu lọc không thể lớn hơn ngày hiện tại.");

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate!.Value)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage("Ngày bắt đầu lọc không thể lớn hơn ngày kết thúc lọc.");
    }
}
