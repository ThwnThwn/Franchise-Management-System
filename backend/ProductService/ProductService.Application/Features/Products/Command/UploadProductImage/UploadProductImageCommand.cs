using Ardalis.Result;
using MediatR;

namespace ProductService.Application.Features.Products.Command.UploadProductImage;
public sealed record UploadProductImageCommand(
    Guid ProductId,
    byte[] ImageData,
    string FileName,
    string ContentType)
: IRequest<Result<UploadProductImageResponse>>;

public sealed record UploadProductImageResponse(
    Guid ProductId,
    string ImageUrl,
    string ImagePublicId
);