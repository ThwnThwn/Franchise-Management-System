using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllProductsAsync(Guid? categoryId = null);
    Task<Product?> GetProductByIdAsync(Guid id);
}
