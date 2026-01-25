using System;
using System.Threading.Tasks;
using SaleService.Domain.Entities;

namespace SaleService.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order> CreateOrderAsync(Order order);
    Task<Order?> GetOrderByIdAsync(Guid orderId);
}
