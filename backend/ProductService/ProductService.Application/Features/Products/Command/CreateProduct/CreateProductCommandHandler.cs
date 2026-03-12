using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Interfaces;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.Products.Command.CreateProduct.CreateProductCommand;

namespace ProductService.Application.Features.Products.Command.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IMapper mapper,
    ICloudinaryService cloudinaryService
) : IRequestHandler<CreateProductCommand, Result<CreateProductCommandResponse>>
{
    public async Task<Result<CreateProductCommandResponse>> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var exist = await productRepository.GetProductByNameAsync(request.Name, null);
        if (exist != null)
        {
            return Result.Conflict($"Đã tồn tại tên sản phẩm: '{request.Name}'.");
        }

        var product = mapper.Map<Product>(request);

        if (request.ImageFile != null)
        {
            var uploadResult = await cloudinaryService.UploadImageAsync(request.ImageFile);
            if (uploadResult != null)
            {
                product.ImageUrl = uploadResult.Url;
                product.ImagePublicId = uploadResult.PublicId;
            }
        }

        await productRepository.AddAsync(product);

        return Result.Success(mapper.Map<CreateProductCommandResponse>(product));
    }
}