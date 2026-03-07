using System.Globalization;
using System.Text;

namespace SalesService.Application.Features.Reports.Queries.GetSalesTrend;

public static class SalesTrendCsvExporter
{
    public static byte[] Export(SalesTrendResponse response)
    {
        var sb = new StringBuilder();

        // ===== HEADER =====
        sb.AppendLine("TimeLabel,OrderVolume,Revenue");

        // ===== ROWS =====
        foreach (var dp in response.DataPoints)
        {
            sb.AppendLine(string.Join(",",
                Escape(dp.TimeLabel),
                dp.OrderVolume,
                dp.Revenue.ToString(CultureInfo.InvariantCulture)
            ));
        }

        // ===== FOOTER =====
        sb.AppendLine();

        var totalRevenue = response.DataPoints.Sum(x => x.Revenue);
        var totalOrders = response.DataPoints.Sum(x => x.OrderVolume);
        sb.AppendLine($"TOTAL,{totalOrders},{totalRevenue.ToString(CultureInfo.InvariantCulture)}");

        if (!string.IsNullOrEmpty(response.PeakPeriodLabel))
            sb.AppendLine($"# {response.PeakPeriodLabel}");

        if (!string.IsNullOrEmpty(response.SlowPeriodLabel))
            sb.AppendLine($"# {response.SlowPeriodLabel}");

        return new UTF8Encoding(true).GetBytes(sb.ToString());
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
