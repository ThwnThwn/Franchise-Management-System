using MediatR;

namespace SalesService.Application.Features.Reports.Queries.GetExpenseReport
{
    public record class GetExpenseReportQuery(GetExpenseReportReq Request) : IRequest<GetExpenseReportResponse>;
    public class GetExpenseReportReq
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public Guid? StoreId { get; set; }
        public bool? Include_csv { get; set; }
    }

    public class GetExpenseReportResponse
    {
        public List<StoreExpenseSummaryDto> Stores { get; set; } = new();
        public decimal GrandTotalExpense { get; set; }
        public byte[]? CsvFile { get; init; }
        public string? CsvFileName { get; init; }

    }
    public class StoreExpenseSummaryDto
    {
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = default!;
        public decimal TotalExpense { get; set; }
        public List<ExpenseReportItemDto> Items { get; set; } = new();
    }
    public class ExpenseReportItemDto
    {
        public string ExpenseCategory { get; set; } = default!;
        public DateOnly Date { get; set; }
        public string Description { get; set; } = default!;
        public decimal Amount { get; set; }
    }

}
