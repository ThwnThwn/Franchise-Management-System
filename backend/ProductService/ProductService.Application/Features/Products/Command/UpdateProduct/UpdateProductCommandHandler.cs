using Ardalis.Result;
using AutoMapper;
using MediatR;
using  ProductService.Application.Features.Products.Command.UpdateProduct;
using ProductService.Application.Repositories;
using static UpdateProductCommand;

namespace ProductService.Application.Features.Products.Command.UpdateProduct
{
    public sealed class UpdateProductCommandHandler :
        IRequestHandler<UpdateProductCommand, Result<UpdateProductCommandResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        public UpdateProductCommandHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<UpdateProductCommandResponse>> Handle(
            UpdateProductCommand request,
            CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetProductByIdAsync(request.ProductId);
            if (product is null)
            {
                return Result.NotFound($"Product with id '{request. ProductId}' not found");
            }
            var checkName = await _productRepository.GetProductByNameAsync(request.Request.Name);
            if (checkName is not null)
            {
                return Result.Conflict($"Product with name '{request.Request.Name}' already exists");
            }

            product.Name = request.Request.Name;
            product.Description = request.Request.Description;
            product.ImageUrl = request.Request.ImageUrl;
            product.ImagePublicId = request.Request.ImagePublicId;
            product.IsActive = request.Request.IsActive;
            product.CategoryId = request.Request.CategoryId;
            product.CreatedAt = product.CreatedAt;
            product.UpdatedAt = DateTimeOffset.UtcNow;
            await _productRepository.UpdateAsync(product);
            return Result.Success(new UpdateProductCommandResponse
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                ImagePublicId = product.ImagePublicId,
                IsActive = (bool)product.IsActive,
                CategoryId = product.CategoryId,
                UpdatedAt = product.UpdatedAt
            });
        }
    }
}
