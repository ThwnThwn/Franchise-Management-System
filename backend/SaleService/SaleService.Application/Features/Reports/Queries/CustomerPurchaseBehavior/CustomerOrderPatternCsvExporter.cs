using SalesService.Application.Features.Reports.Queries.CustomerPurchaseBehavior;
using System.Globalization;
using System.Text;

public static class CustomerOrderPatternCsvExporter
{
    public static byte[] Export(List<CustomerOrderPatternResponse> items)
    {
        var sb = new StringBuilder();

        // ===== HEADER =====
        sb.AppendLine("Customer ID,Full Name,Email,Phone,Total Orders,Average Order Value,Total Spent,Last Order Date");

        // ===== ROWS =====
        foreach (var item in items)
        {
            sb.AppendLine(string.Join(",",
                Escape(item.CustomerId.ToString()),
                Escape(item.FullName),
                Escape(item.Email ?? ""),
                Escape(item.Phone ?? ""),
                item.TotalOrders,
                $"\"{item.AverageOrderValue.ToString("N0", CultureInfo.InvariantCulture)}\"",
                $"\"{item.TotalSpent.ToString("N0", CultureInfo.InvariantCulture)}\"",
                item.LastOrderDate.ToString("yyyy-MM-dd HH:mm:ss")
            ));
        }

        // ===== FOOTER =====
        sb.AppendLine();
        sb.AppendLine($",,,,Total Customers,{items.Count}");
        sb.AppendLine($",,,,Total Orders,{items.Sum(x => x.TotalOrders)}");
        sb.AppendLine($",,,,Overall Total Spent,\"{items.Sum(x => x.TotalSpent).ToString("N0", CultureInfo.InvariantCulture)}\"");

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