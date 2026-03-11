namespace SalesService.Application.Features.Reports.Queries.CustomerPurchaseBehavior
{
    
    public sealed class CustomerOrderStatDto
    {
        public Guid CustomerId { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTimeOffset LastOrderDate { get; set; }
    }
}
