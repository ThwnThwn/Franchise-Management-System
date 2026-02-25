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
            var tmpProd = await _productRepository
                .GetProductByNameAsync(request.Request.Name ?? "", request.ProductId);
            if (tmpProd is not null)
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
                IsActive = request.Request.IsActive,
                CategoryId = request.Request.CategoryId,
                ProductVariants = request.Request.Variants?.Select(v => new Domain.Entities.ProductVariant
                {
                    Sku = v.Sku ?? "",
                    Name = v.Name ?? "",
                    Attributes = v.Attributes,
                    BasePrice = v.BasePrice ?? 0,
                    Cost = v.Cost ?? 0,
                    IsActive = v.IsActive ?? true
                }).ToList() ?? new List<Domain.Entities.ProductVariant>()
            };

            await _productRepository.UpdateWithVariantsAsync(product);
            return Result.Success(_mapper.Map<UpdateProductCommandResponse>(product));
        }
    }
}