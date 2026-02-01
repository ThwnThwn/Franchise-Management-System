using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Features.Products.Command.UpdateProduct;
using ProductService.Application.Repositories;
using static UpdateProductCommand;

namespace ProductService.Application.Features.Products.Command.UpdateProduct
{
    public sealed class UpdateProductCommandHandler :
        IRequestHandler<UpdateProductCommand, Result<UpdateProductCommandResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public UpdateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper
        )
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<Result<UpdateProductCommandResponse>> Handle(
            UpdateProductCommand request,
            CancellationToken cancellationToken)
        {
            var checkName = await _productRepository
                .GetProductByNameAsync(request.Request.Name ?? "", request.ProductId);
            if (checkName is not null)
            {
                return Result.Conflict($"Product with name '{request.Request.Name}' already exists");
            }
            var checkCategory = await _categoryRepository.GetByIdAsync(request.Request.CategoryId);
            if (checkCategory is null)
            {
                return Result.NotFound($"Category with id '{request.Request.CategoryId}' not found");
            }
            var product = new Domain.Entities.Product
            {
                Id = request.ProductId,
                Name = request.Request.Name ?? "",
                Description = request.Request.Description,
                ImageUrl = request.Request.ImageUrl,
                ImagePublicId = request.Request.ImagePublicId,
                IsActive = request.Request.IsActive,
                CategoryId = request.Request.CategoryId
            };
            await _productRepository.UpdateAsync(product);
            return Result.Success(_mapper.Map<UpdateProductCommandResponse>(product));
        }
    }
}