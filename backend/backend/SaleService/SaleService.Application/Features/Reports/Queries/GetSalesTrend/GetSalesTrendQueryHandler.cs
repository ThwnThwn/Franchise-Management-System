using Ardalis.Result;
using MediatR;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Persisitences.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Features.Reports.Queries.GetSalesTrend;

public class GetSalesTrendQueryHandler : IRequestHandler<GetSalesTrendQuery, Result<SalesTrendResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductServiceClient _productServiceClient;

    public GetSalesTrendQueryHandler(IOrderRepository orderRepository, IProductServiceClient productServiceClient)
    {
        _orderRepository = orderRepository;
        _productServiceClient = productServiceClient;
    }

    public async Task<Result<SalesTrendResponse>> Handle(GetSalesTrendQuery request, CancellationToken cancellationToken)
    {
        List<string>? categorySkus = null;

        if (request.CategoryId.HasValue)
        {
            categorySkus = await _productServiceClient.GetSkusByCategoryAsync(request.CategoryId.Value);

            if (categorySkus == null || !categorySkus.Any())
            {
                return Result<SalesTrendResponse>.Success(new SalesTrendResponse());
            }
        }

        var dbData = await _orderRepository.GetSalesTrendAsync(
            request.FromDate,
            request.ToDate,
            request.Aggregation,
            categorySkus,
            request.StoreId,
            cancellationToken);

        // Fill data gaps
        var filledData = FillDataGaps(dbData, request.FromDate.AddHours(7).DateTime, request.ToDate.AddHours(7).DateTime, request.Aggregation);

        var response = new SalesTrendResponse
        {
            DataPoints = filledData
        };

        if (filledData.Any(x => x.OrderVolume > 0))
        {
            var peak = filledData.MaxBy(x => x.Revenue);
            var slow = filledData.Where(x => x.OrderVolume > 0).MinBy(x => x.Revenue); // Only consider periods with at least 1 order for slowness, or min over all? Typically we include 0 as slow.

            response.PeakPeriodLabel = peak != null && peak.Revenue > 0 ? $"Peak: {peak.TimeLabel} ({peak.Revenue:N0}đ)" : null;

            var slowAbs = filledData.MinBy(x => x.Revenue);
            response.SlowPeriodLabel = slowAbs != null ? $"Slow: {slowAbs.TimeLabel} ({slowAbs.Revenue:N0}đ)" : null;
        }

        return Result<SalesTrendResponse>.Success(response);
    }

    private List<SalesTrendDataPointDto> FillDataGaps(List<SalesTrendDataPointDto> source, DateTime startLocal, DateTime endLocal, TimeAggregation agg)
    {
        var dict = source.ToDictionary(x => x.TimeLabel, x => x);
        var result = new List<SalesTrendDataPointDto>();

        DateTime current = startLocal;

        while (current <= endLocal)
        {
            string label = "";
            DateTime next = current;

            if (agg == TimeAggregation.Hour)
            {
                label = $"{current:yyyy-MM-dd} {current.Hour:D2}:00";
                next = current.AddHours(1);
            }
            else if (agg == TimeAggregation.Day)
            {
                label = current.ToString("yyyy-MM-dd");
                next = current.AddDays(1);
            }
            else if (agg == TimeAggregation.Week)
            {
                var isoDate = GetIso8601YearAndWeek(current);
                label = $"Week {isoDate.Week}, {isoDate.Year}";
                next = current.AddDays(7);
                // Align to Monday to ensure proper jumping
                if (current.DayOfWeek != DayOfWeek.Monday)
                {
                    int diff = (7 + (current.DayOfWeek - DayOfWeek.Monday)) % 7;
                    next = current.AddDays(7 - diff);
                }
            }

            if (dict.TryGetValue(label, out var existing))
            {
                result.Add(existing);
            }
            else
            {
                // Must ensure we only add unique week labels if navigating by days inside a week
                if (!result.Any(x => x.TimeLabel == label))
                {
                    result.Add(new SalesTrendDataPointDto
                    {
                        TimeLabel = label,
                        OrderVolume = 0,
                        Revenue = 0
                    });
                }
            }

            current = next;
        }

        // Add the end edge case if weeks misalignment causes it to be skipped
        if (agg == TimeAggregation.Week)
        {
            var endIsoDate = GetIso8601YearAndWeek(endLocal);
            string endLabel = $"Week {endIsoDate.Week}, {endIsoDate.Year}";
            if (!result.Any(x => x.TimeLabel == endLabel))
            {
                if (dict.TryGetValue(endLabel, out var existing))
                {
                    result.Add(existing);
                }
                else
                {
                    result.Add(new SalesTrendDataPointDto
                    {
                        TimeLabel = endLabel,
                        OrderVolume = 0,
                        Revenue = 0
                    });
                }
            }
        }

        return result;
    }

    private static (int Year, int Week) GetIso8601YearAndWeek(DateTime time)
    {
        var day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            time = time.AddDays(3);
        }
        var week = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return (time.Year, week);
    }
}
