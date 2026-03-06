using Microsoft.EntityFrameworkCore;
using SalesService.Application.Features.Reports.Queries.ProductPerformance;
using SalesService.Application.Persisitences.OrderItems;
using SalesService.Domain.Entities;
using SalesService.Infrastructure.ApplicationDbContext;

namespace SalesService.Infrastructure.Repositories.OrderItems
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly ISalesDbContext _context;

        public OrderItemRepository(ISalesDbContext context)
        {
            _context = context;
        }

        public void RemoveOrderItems(List<OrderItem> items)
        {
            _context.OrderItems.RemoveRange(items);
        }

        public void AddOrderItems(List<OrderItem> items)
        {
            _context.OrderItems.AddRange(items);
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<OrderItem>> GetProductSalesStatisticAsync(
            Guid storeId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            List<Guid>? productIds,
            CancellationToken ct)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi =>
                    oi.Order != null &&
                    oi.Order.StoreId == storeId &&
                    oi.Order.Status == Domain.Enums.OrderStatus.Completed &&
                    !oi.IsDeleted &&
                    !oi.Order.IsDeleted)
                .AsNoTracking();

            if (from.HasValue)
                query = query.Where(oi => oi.Order!.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(oi => oi.Order!.CreatedAt <= to.Value);

            if (productIds != null && productIds.Any())
                query = query.Where(oi => productIds.Contains(oi.ProductId));

            return await query.ToListAsync(ct);
        }

        public async Task<List<ProductCancelStatDto>> GetProductCancelStatisticAsync(
        Guid storeId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        List<Guid>? productIds,
        CancellationToken ct)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi =>
                    oi.Order != null &&
                    oi.Order.StoreId == storeId &&
                    !oi.IsDeleted &&
                    !oi.Order.IsDeleted)
                .AsNoTracking();

            if (from.HasValue)
                query = query.Where(oi => oi.Order!.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(oi => oi.Order!.CreatedAt <= to.Value);

            if (productIds != null && productIds.Any())
                query = query.Where(oi => productIds.Contains(oi.ProductId));

            var result = await query
                .GroupBy(oi => oi.ProductId)
                .Select(g => new ProductCancelStatDto
                {
                    ProductId = g.Key,
                    TotalOrders = g.Select(oi => oi.OrderId).Distinct().Count(),
                    TotalCancelled = g
                        .Where(oi => oi.Order!.Status == Domain.Enums.OrderStatus.Cancelled)
                        .Select(oi => oi.OrderId)
                        .Distinct()
                        .Count()
                })
                .ToListAsync(ct);

            return result;
        }
    }
}
