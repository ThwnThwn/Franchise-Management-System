using Ardalis.Result;
using MediatR;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Products.Command.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository
) : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId);

        if (product == null)
        {
            return Result.NotFound($"Không tìm thấy sản phẩm với mã: '{request.ProductId}'.");
        }

        try
        {
            await productRepository.DeleteAsync(product.Id);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Error($"Đã xảy ra lỗi khi xóa sản phẩm: {ex.Message}");
        }
    }
}