using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.Products.Command.CreateProduct.CreateProductCommand;
using ProductService.Application.Interfaces;

namespace ProductService.Application.Features.Products.Command.CreateProduct;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Result<CreateProductCommandResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;

    public CreateProductCommandHandler(IProductRepository productRepository, IMapper mapper, ICloudinaryService cloudinaryService)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
    }

    /// <summary>
    /// Handle the creation of a new product.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

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

        // Thêm ảnh vào cho sản phẩm

        if (request.ImageFile != null)
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(request.ImageFile);
            if (uploadResult != null)
            {
                product.ImageUrl = uploadResult.Url;
                product.ImagePublicId = uploadResult.PublicId;
            }
        }

        await _productRepository.AddAsync(product);

        return Result.Success(_mapper.Map<CreateProductCommandResponse>(product));
    }
}