using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.Products.Command.CreateProduct.CreateProductCommand;

namespace ProductService.Application.Features.Products.Command.CreateProduct;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Result<CreateProductCommandResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<CreateProductCommandResponse>> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var exist = await _productRepository.GetProductByNameAsync(request.Name);
        if (exist != null)
        {
            return Result.Conflict(
                $"Product with name '{request.Name}' already exists.");
        }

        var product = _mapper.Map<Product>(request);

        await _productRepository.AddAsync(product);

        return Result.Success(_mapper.Map<CreateProductCommandResponse>(product));
    }
}
