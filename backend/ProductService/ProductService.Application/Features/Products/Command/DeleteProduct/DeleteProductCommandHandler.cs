using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;
namespace ProductService.Application.Features.Products.Command.DeleteProduct
{
    public sealed class DeleteProductCommandHandler :
        IRequestHandler<DeleteProductCommand, Result<bool>>
    {
        private readonly IProductRepository _productRepository;
        public DeleteProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Result<bool>> Handle(
            DeleteProductCommand request,
            CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return Result.NotFound(
                    $"Product with ID '{request.ProductId}' not found.");
            }
            await _productRepository.DeleteAsync(product.Id);
            return Result.Success(true);
        }
    }
}
