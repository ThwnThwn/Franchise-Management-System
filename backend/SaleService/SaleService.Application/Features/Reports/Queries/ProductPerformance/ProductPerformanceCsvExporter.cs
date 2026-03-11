using SalesService.Application.Features.Reports.Queries.ProductPerformance;
using System.Globalization;
using System.Text;

public static class ProductPerformanceCsvExporter
{
    public static byte[] Export(List<ProductPerformanceResponse> items)
    {
        var sb = new StringBuilder();

        // ===== HEADER =====
        sb.AppendLine("Product ID,Product Name,Category ID,Category Name,Units Sold,Revenue (VND),Total Orders,Total Cancelled,Cancel Rate (%),Tag");

        // ===== ROWS =====
        foreach (var item in items)
        {
            sb.AppendLine(string.Join(",",
                Escape(item.ProductId.ToString()),
                Escape(item.ProductName),
                Escape(item.CategoryId.ToString()),
                Escape(item.CategoryName ?? ""),
                item.UnitsSold,
                $"\"{item.Revenue.ToString("N0", CultureInfo.InvariantCulture)}\"",
                item.TotalOrders,
                item.TotalCancelled,
                $"{item.CancelledRate.ToString("F2", CultureInfo.InvariantCulture)}%",
                item.Tag.ToString()
            ));
        }

        // ===== FOOTER =====
        sb.AppendLine();
        sb.AppendLine($",,,,Total Products,{items.Count}");
        sb.AppendLine($",,,,Total Units Sold,{items.Sum(x => x.UnitsSold)}");
        sb.AppendLine($",,,,Overall Revenue,\"{items.Sum(x => x.Revenue).ToString("N0", CultureInfo.InvariantCulture)}\"");

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