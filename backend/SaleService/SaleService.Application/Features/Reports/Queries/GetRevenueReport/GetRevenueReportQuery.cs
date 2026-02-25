using Ardalis.Result;
using MediatR;
using System;

namespace SalesService.Application.Features.Reports.Queries.GetRevenueReport;

public class GetRevenueReportQuery : IRequest<Result<RevenueReportResponse>>
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? StoreId { get; set; }
    public bool? Include_csv { get; set; }

    public GetRevenueReportQuery(DateOnly? fromDate, DateOnly? toDate, Guid? categoryId, Guid? storeId)
    {
        FromDate = fromDate;
        ToDate = toDate;
        CategoryId = categoryId;
        StoreId = storeId;
    }
}
