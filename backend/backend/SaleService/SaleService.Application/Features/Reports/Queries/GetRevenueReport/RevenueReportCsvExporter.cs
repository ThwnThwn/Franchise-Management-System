using SalesService.Application.Features.Reports.Queries.GetRevenueReport;
using System.Globalization;
using System.Text;

namespace SalesService.Application.Features.Reports.Queries.GetRevenueReport;
public static class RevenueReportCsvExporter
{
    public static byte[] Export(RevenueReportResponse report)
    {
        var sb = new StringBuilder();

        bool includeStoreColumn = report.StoreId == null;

        // ===== HEADER =====
        if (includeStoreColumn)
        {
            sb.AppendLine("StoreId,ProductName,Sku,QuantitySold,UnitPrice,TotalRevenue");
        }
        else
        {
            sb.AppendLine("ProductName,Sku,QuantitySold,UnitPrice,TotalRevenue");
        }

        // ===== ROWS =====
        foreach (var item in report.Items)
        {
            if (includeStoreColumn)
            {
                sb.AppendLine(string.Join(",",
                    Escape(item.StoreId?.ToString() ?? string.Empty), // <-- must exist in DTO
                    Escape(item.ProductName),
                    Escape(item.Sku),
                    item.QuantitySold,
                    item.UnitPrice.ToString(CultureInfo.InvariantCulture),
                    item.TotalRevenue.ToString(CultureInfo.InvariantCulture)
                ));
            }
            else
            {
                sb.AppendLine(string.Join(",",
                    Escape(item.ProductName),
                    Escape(item.Sku),
                    item.QuantitySold,
                    item.UnitPrice.ToString(CultureInfo.InvariantCulture),
                    item.TotalRevenue.ToString(CultureInfo.InvariantCulture)
                ));
            }
        }

        // ===== FOOTER =====
        sb.AppendLine();

        if (includeStoreColumn)
        {
            sb.AppendLine($",,,,OverallTotal,{report.OverallTotalRevenue.ToString(CultureInfo.InvariantCulture)}");
        }
        else
        {
            sb.AppendLine($",,,OverallTotal,{report.OverallTotalRevenue.ToString(CultureInfo.InvariantCulture)}");
        }

        return new UTF8Encoding(true).GetBytes(sb.ToString());
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}