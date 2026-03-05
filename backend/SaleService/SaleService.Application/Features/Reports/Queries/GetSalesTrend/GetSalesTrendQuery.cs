using Ardalis.Result;
using MediatR;
using System;

namespace SalesService.Application.Features.Reports.Queries.GetSalesTrend;

public class GetSalesTrendQuery : IRequest<Result<SalesTrendResponse>>
{
    public DateTimeOffset FromDate { get; set; }
    public DateTimeOffset ToDate { get; set; }
    public TimeAggregation Aggregation { get; set; }
    
    public Guid? StoreId { get; set; }
    public Guid? CategoryId { get; set; }
}
