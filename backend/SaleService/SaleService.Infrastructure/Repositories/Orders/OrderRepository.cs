using Microsoft.EntityFrameworkCore;
using SalesService.Application.Features.Reports.Queries.GetSalesTrend;
using SalesService.Application.Features.Orders.Queries.GetByIds;
using SalesService.Application.Features.Orders.Queries.GetOrderVolume;
using SalesService.Application.Features.Reports.Queries.GetRevenueReport;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.ApplicationDbContext;

namespace SalesService.Infrastructure.Repositories.Orders;

public class OrderRepository : IOrderRepository
{
    private readonly ISalesDbContext _context;

    public OrderRepository(ISalesDbContext context)
    {
        _context = context;
    }
    /// <summary>
    /// Creates the asynchronous.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }
    /// <summary>
    /// Gets the by identifier asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderSaleItems)
            .FirstOrDefaultAsync(
                o => o.Id == id && !o.IsDeleted,
                cancellationToken);
    }
    /// <summary>
    /// Gets the orders by customer identifier asynchronous.
    /// </summary>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="page">The page.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
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
    /// <summary>
    /// Counts the orders by customer identifier asynchronous.
    /// </summary>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<int> CountOrdersByCustomerIdAsync(
    Guid customerId,
    CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .CountAsync(cancellationToken);
    }
    /// <summary>
    /// Updates the asynchronous.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    /// <summary>
    /// Gets the orders asynchronous.
    /// </summary>
    /// <param name="storeId">The store identifier.</param>
    /// <param name="code">The code.</param>
    /// <param name="fromDate">From date.</param>
    /// <param name="toDate">To date.</param>
    /// <param name="status">The status.</param>
    /// <param name="paymentMethod">The payment method.</param>
    /// <param name="type">The type.</param>
    /// <param name="page">The page.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<(List<Order> Items, int TotalCount)> GetOrdersAsync(
        Guid? storeId,
        string? code,
        DateOnly? fromDate,
        DateOnly? toDate,
        OrderStatus? status,
        PaymentMethod? paymentMethod,
        OrderType? type,
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
        if (type.HasValue)
        {
            query = query.Where(o => o.Type == type.Value);
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
    /// <summary>
    /// Saves the asynchronous.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    /// <summary>
    /// Gets the revenue report asynchronous.
    /// </summary>
    /// <param name="fromDate">From date.</param>
    /// <param name="toDate">To date.</param>
    /// <param name="skus">The skus.</param>
    /// <param name="storedId">The stored identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
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
                StoreId = g.Key.StoreId, // multi-store
                Sku = g.Key.Sku,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(x => x.Quantity),
                UnitPrice = g.Max(x => x.UnitPrice),
                TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .ToListAsync(cancellationToken);

        return aggregatedResult;
    }
    /// <summary>
    /// Gets the order volume asynchronous.
    /// </summary>
    /// <param name="fromDate">From date.</param>
    /// <param name="toDate">To date.</param>
    /// <param name="storeId">The store identifier.</param>
    /// <param name="status">The status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<List<OrderVolumePerDayDto>> GetOrderVolumeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        Guid? storeId,
        OrderStatus? status,
        CancellationToken cancellationToken)
    {
        var fromUtc = new DateTimeOffset(
            fromDate.ToDateTime(TimeOnly.MinValue),
            TimeSpan.Zero);

        var toUtc = new DateTimeOffset(
            toDate.ToDateTime(TimeOnly.MaxValue),
            TimeSpan.Zero);

        var query = _context.Orders
            .AsNoTracking()
            .Where(oi => oi.CreatedAt >= fromUtc && oi.CreatedAt <= toUtc);

        if (storeId.HasValue)
        {
            query = query.Where(oi => oi.StoreId == storeId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(oi => oi.Status == status.Value);
        }
        var data = await query
            .GroupBy(o => new { o.CreatedAt.Date, o.StoreId })
            .Select(g => new
            {
                Date = g.Key.Date,
                StoreId = g.Key.StoreId,
                TotalOrders = g.Count(),
                CanceledOrders = g.Count(o => o.Status == OrderStatus.Cancelled),
                CompletedOrders = g.Count(o => o.Status == OrderStatus.Completed),
                StatusGroups = g
                    .GroupBy(o => o.Status)
                    .Select(sg => new
                    {
                        Status = sg.Key,
                        Count = sg.Count()
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return data.Select(x => new OrderVolumePerDayDto
        {
            Date = DateOnly.FromDateTime(x.Date),
            StoreId = x.StoreId,
            TotalOrders = x.TotalOrders,
            CanceledOrders = x.CanceledOrders,
            CompletedOrders = x.CompletedOrders,
            StatusCounts = x.StatusGroups
            .ToDictionary(
                s => s.Status,
                s => s.Count
            )
        }).ToList();
    }

    /// <summary>
    /// Gets the orders by type asynchronous.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<List<Order>> GetOrdersByTypeAsync(OrderType type, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Where(o => o.Type == type && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Existses the order type asynchronous.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<bool> ExistsOrderTypeAsync(OrderType type, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AnyAsync(o => o.Type == type && !o.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets the by user ids asynchronous.
    /// </summary>
    /// <param name="userIds">The user ids.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<IEnumerable<GetByUserIdsRes>> GetByUserIdsAsync(List<Guid> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds == null || !userIds.Any())
            return new List<GetByUserIdsRes>();

        var orderCounts = await _context.Orders
        .AsNoTracking()
        .Where(o => o.CustomerId.HasValue &&
            userIds.Contains(o.CustomerId.Value))
        .GroupBy(o => o.CustomerId.Value)
        .Select(g => new
        {
            UserId = g.Key,
            Count = g.Count()
        })
        .ToListAsync(cancellationToken);

        var dictionary = orderCounts.ToDictionary(x => x.UserId, x => x.Count);

        return userIds.Select(id => new GetByUserIdsRes
        {
            UserId = id,
            OrderCount = dictionary.TryGetValue(id, out var count) ? count : 0
        }).ToList();
    }
    /// <summary>
    /// Gets the sales trend asynchronous.
    /// </summary>
    /// <param name="fromDate">From date.</param>
    /// <param name="toDate">To date.</param>
    /// <param name="aggregation">The aggregation.</param>
    /// <param name="categorySkus">The category skus.</param>
    /// <param name="storeId">The store identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<List<SalesTrendDataPointDto>> GetSalesTrendAsync(
        DateTimeOffset fromDate,
        DateTimeOffset toDate,
        TimeAggregation aggregation,
        List<string>? categorySkus,
        Guid? storeId,
        CancellationToken cancellationToken = default)
    {
        var fromUtc = fromDate.ToUniversalTime();
        var toUtc = toDate.ToUniversalTime();

        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc && o.Status == OrderStatus.Completed && !o.IsDeleted);

        if (storeId.HasValue)
            query = query.Where(o => o.StoreId == storeId.Value);

        if (categorySkus != null && categorySkus.Any())
        {
            query = query.Where(o => o.OrderItems.Any(oi => categorySkus.Contains(oi.Sku)));
        }

        // Offset to Vietnam Time (+7)
        var offsetQuery = query.Select(o => new
        {
            LocalTime = o.CreatedAt.AddHours(7),
            Amount = o.TotalAmount
        });

        if (aggregation == TimeAggregation.Hour)
        {
            var grouped = await offsetQuery
                .GroupBy(x => new { x.LocalTime.Date, x.LocalTime.Hour })
                .Select(g => new
                {
                    Date = g.Key.Date,
                    Hour = g.Key.Hour,
                    Volume = g.Count(),
                    Revenue = g.Sum(x => x.Amount)
                })
                .ToListAsync(cancellationToken);

            return grouped.Select(x => new SalesTrendDataPointDto
            {
                TimeLabel = $"{x.Date:yyyy-MM-dd} {x.Hour:D2}:00",
                OrderVolume = x.Volume,
                Revenue = x.Revenue
            }).OrderBy(x => x.TimeLabel).ToList();
        }

        // For both Day and Week, we fetch daily aggregations first
        var dailyGrouped = await offsetQuery
            .GroupBy(x => x.LocalTime.Date)
            .Select(g => new
            {
                Date = g.Key,
                Volume = g.Count(),
                Revenue = g.Sum(x => x.Amount)
            })
            .ToListAsync(cancellationToken);

        if (aggregation == TimeAggregation.Day)
        {
            return dailyGrouped.Select(x => new SalesTrendDataPointDto
            {
                TimeLabel = x.Date.ToString("yyyy-MM-dd"),
                OrderVolume = x.Volume,
                Revenue = x.Revenue
            }).OrderBy(x => x.TimeLabel).ToList();
        }

        var weekGrouped = dailyGrouped
            .GroupBy(x => GetIso8601YearAndWeek(x.Date))
            .Select(g => new SalesTrendDataPointDto
            {
                TimeLabel = $"Week {g.Key.Week}, {g.Key.Year}",
                OrderVolume = g.Sum(x => x.Volume),
                Revenue = g.Sum(x => x.Revenue)
            })
            .OrderBy(x => x.TimeLabel)
            .ToList();

        return weekGrouped;
    }
    /// <summary>
    /// Gets the iso8601 year and week.
    /// </summary>
    /// <param name="time">The time.</param>
    /// <returns></returns>
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
