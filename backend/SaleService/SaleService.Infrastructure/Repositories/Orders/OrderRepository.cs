using Microsoft.EntityFrameworkCore;
using SalesService.Application.Features.Reports.Queries.GetRevenueReport;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.ApplicationDbContext;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Infrastructure.Repositories.Orders;

public class OrderRepository : IOrderRepository
{
    private readonly ISalesDbContext _context;

    public OrderRepository(ISalesDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(
                o => o.Id == id && !o.IsDeleted,
                cancellationToken);
    }

    public async Task<List<Order>> GetOrdersByCustomerIdAsync(
    Guid customerId,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountOrdersByCustomerIdAsync(
    Guid customerId,
    CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .CountAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(List<Order> Items, int TotalCount)> GetOrdersAsync(
        Guid? storeId,
        string? code,
        DateOnly? fromDate,
        DateOnly? toDate,
        OrderStatus? status,
        PaymentMethod? paymentMethod,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsQueryable();

        if (storeId.HasValue)
        {
            query = query.Where(o => o.StoreId == storeId.Value);
        }

        if (!string.IsNullOrEmpty(code))
        {
            query = query.Where(o => o.Code.Contains(code));
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (paymentMethod.HasValue)
        {
            query = query.Where(o => o.PaymentMethod == paymentMethod.Value);
        }

        if (fromDate.HasValue)
        {
            // Chuyển đổi DateOnly sang DateTimeOffset (bắt đầu ngày 00:00:00 +07:00) rồi chuyển sang UTC
            var fromDateTime = new DateTimeOffset(fromDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(7)).ToUniversalTime();
            query = query.Where(o => o.CreatedAt >= fromDateTime);
        }

        if (toDate.HasValue)
        {
            // Chuyển đổi DateOnly sang DateTimeOffset (kết thúc ngày 23:59:59.999 +07:00) rồi chuyển sang UTC
            var toDateTime = new DateTimeOffset(toDate.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.FromHours(7)).ToUniversalTime();
            query = query.Where(o => o.CreatedAt <= toDateTime);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<RevenueItemDto>> GetRevenueReportAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        List<string>? skus,
        Guid? storedId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.OrderItems
        .Include(oi => oi.Order)
        .Where(oi => oi.Order != null
            && oi.Order.Status == OrderStatus.Completed
            && !oi.IsDeleted
            && !oi.Order.IsDeleted)
        .AsNoTracking();

        if (fromDate.HasValue)
        {
            var fromDateTime = new DateTimeOffset(fromDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(7)).ToUniversalTime();
            query = query.Where(oi => oi.Order!.CreatedAt >= fromDateTime);
        }

        if (toDate.HasValue)
        {
            var toDateTime = new DateTimeOffset(toDate.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.FromHours(7)).ToUniversalTime();
            query = query.Where(oi => oi.Order!.CreatedAt <= toDateTime);
        }

        if (skus != null)
        {
            query = query.Where(oi => skus.Contains(oi.Sku));
        }

        if (storedId.HasValue)
        {
            query = query.Where(oi => oi.Order!.StoreId == storedId.Value);
        }

        var aggregatedResult = storedId.HasValue
        ? await query
            .GroupBy(oi => new
            {
                oi.Sku,
                oi.ProductName
            })
            .Select(g => new RevenueItemDto
            {
                StoreId = storedId.Value, // single store
                Sku = g.Key.Sku,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(x => x.Quantity),
                UnitPrice = g.Max(x => x.UnitPrice),
                TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .ToListAsync(cancellationToken)

        : await query
            .GroupBy(oi => new
            {
                oi.Order!.StoreId,
                oi.Sku,
                oi.ProductName
            })
            .Select(g => new RevenueItemDto
            {
                StoreId =g.Key.StoreId, // multi-store
                Sku = g.Key.Sku,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(x => x.Quantity),
                UnitPrice = g.Max(x => x.UnitPrice),
                TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .ToListAsync(cancellationToken);

        return aggregatedResult;
    }
}