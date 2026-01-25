using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SaleService.Application.Interfaces;
using SaleService.Domain.Entities;
using SaleService.Infrastructure.Data;

namespace SaleService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly SalesDbContext _context;

    public OrderRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.IsDeleted == false);
    }
}
