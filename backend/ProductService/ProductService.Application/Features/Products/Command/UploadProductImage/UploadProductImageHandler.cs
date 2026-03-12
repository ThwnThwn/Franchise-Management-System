using Ardalis.Result;
using MediatR;
using ProductService.Application.Interfaces;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Products.Command.UploadProductImage;

public class UploadProductImageHandler(
    IProductRepository productRepository,
    ICloudinaryService cloudinaryService
) : IRequestHandler<UploadProductImageCommand, Result<UploadProductImageResponse>>
{
    public async Task<Result<UploadProductImageResponse>> Handle(
        UploadProductImageCommand request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductByIdAsync(request.ProductId);
        if (product is null)
        {
            return Result.NotFound("Không tìm thấy sản phẩm để cập nhật ảnh.");
        }

        var oldPublicId = product.ImagePublicId;

        try
        {
            var uploadResult = await cloudinaryService.UploadImageFromBytesAsync(
                request.ImageData,
                request.FileName,
                request.ContentType);

            product.ImageUrl = uploadResult.Url;
            product.ImagePublicId = uploadResult.PublicId;

            await productRepository.UpdateAsync(product);

            if (!string.IsNullOrWhiteSpace(oldPublicId))
            {
                await cloudinaryService.DeleteImageAsync(oldPublicId);
            }

            return Result.Success(new UploadProductImageResponse(
                ProductId: product.Id,
                ImageUrl: product.ImageUrl,
                ImagePublicId: product.ImagePublicId
            ));
        }
        catch (Exception ex)
        {
            return Result.Error($"Lỗi khi xử lý hình ảnh: {ex.Message}");
        }
    }
}