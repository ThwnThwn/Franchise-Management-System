using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Products.Queries.GetByProductName;

public sealed class GetProductByNameQueryHandler
    : IRequestHandler<GetProductByNameQuery, Result<GetProductsByNameResult>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductByNameQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<GetProductsByNameResult>> Handle(
    GetProductByNameQuery request,
    CancellationToken cancellationToken)
    {
        var products = await _productRepository
            .GetProductByNameContainingAsync(request.productName);

        if (products == null || !products.Any())
        {
            return Result.NotFound(
                $"No product found with name '{request.productName}'");
        }

        var resultProducts = products.Select(p => new GetProductByNameQueryResponse
        {
            ProductId = p.Id,
            Name = p.Name,
            Description = p.Description ?? string.Empty,
            ImageUrl = p.ImageUrl ?? string.Empty,
            ImagePublicId = p.ImagePublicId ?? string.Empty,
            IsActive = (bool)p.IsActive,
            CategoryId = p.CategoryId,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            Variants = p.ProductVariants?.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                Sku = v.Sku,
                Name = v.Name,
                Attributes = v.Attributes,
                BasePrice = v.BasePrice,
                Cost = v.Cost,
                IsActive = v.IsActive
            }).ToList() ?? new()
        }).ToList();

        return Result.Success(new GetProductsByNameResult
        {
            Products = resultProducts,
            TotalCount = resultProducts.Count
        });
    }

}
