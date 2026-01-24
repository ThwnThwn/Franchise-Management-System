using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ProductService.Application.DTOs.Requests;
using ProductService.Application.DTOs.Responses;
using ProductService.Application.Interfaces;

namespace ProductService.Application.Services;

public class ProductApplicationService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductApplicationService(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public Task<ProductListResponse> GetProductsAsync(Guid? categoryId = null)
    {
        return GetProductsAsync(new GetProductsRequest { CategoryId = categoryId });
    }

    public async Task<ProductListResponse> GetProductsAsync(GetProductsRequest request)
    {
        var products = await _productRepository.GetAllProductsAsync(request.CategoryId);

        var productDtos = _mapper.Map<List<ProductResponse>>(products);

        return new ProductListResponse
        {
            Products = productDtos,
            TotalCount = productDtos.Count
        };
    }

    public Task<ProductResponse?> GetProductByIdAsync(Guid id)
    {
        return GetProductByIdAsync(new GetProductByIdRequest { Id = id });
    }

    public async Task<ProductResponse?> GetProductByIdAsync(GetProductByIdRequest request)
    {
        var product = await _productRepository.GetProductByIdAsync(request.Id);
        
        if (product == null)
            return null;

        return _mapper.Map<ProductResponse>(product);
    }
}
