using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Products.Queries.GetById;

public sealed class GetProductByIdHandler
    : IRequestHandler<GetProductByIdQuery, Result<GetProductDetailDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<GetProductDetailDto>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetProductByIdAsync(request.ProductId);

        if (product is null)
        {
            return Result.NotFound($"Product with id {request.ProductId} not found.");
        }

        var dto = _mapper.Map<GetProductDetailDto>(product);
        return Result.Success(dto);
    }
}

