using SalesService.Application.Features.Orders.Queries.GetOrderVolume;
using SalesService.Domain.Enums;
using System.Globalization;
using System.Text;

namespace SalesService.Application.Features.Orders.Queries.GetOrderVolume;
public static class OrderVolumeCsvExporter
{
    public static byte[] Export(IEnumerable<OrderVolumePerDayDto> orders)
    {
        var sb = new StringBuilder();
        var statuses = Enum.GetValues<OrderStatus>();
        // HEADER
        sb.Append("Date,StoreId,TotalOrders");
        foreach (var s in statuses)
            sb.Append($",{s}");
        sb.AppendLine();

        // ROWS
        foreach (var d in orders)
        {
            sb.Append($"{d.Date:yyyy-MM-dd},{d.StoreId},{d.TotalOrders}");

            foreach (var s in statuses)
            {
                d.StatusCounts.TryGetValue(s, out var count);
                sb.Append($",{count}");
            }

            sb.AppendLine();
        }

        return new UTF8Encoding(true).GetBytes(sb.ToString());
    }
}