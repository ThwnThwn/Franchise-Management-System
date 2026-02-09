using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.Products.Command.CreateProduct;
using ProductService.Application.Features.Products.Command.DeleteProduct;
using ProductService.Application.Features.Products.Queries.GetById;
using ProductService.Application.Features.Products.Queries.GetByKeyword;
using ProductService.Application.Features.Products.Queries.GetByProductName;
using ProductService.Application.Features.Products.Queries.GetList;
using ProductService.Application.Interfaces;
using ProductService.Application.Repositories;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IProductRepository _productRepository;

    public ProductsController(
        IMediator mediator,
        ICloudinaryService cloudinaryService,
        IProductRepository productRepository)
    {
        _mediator = mediator;
        _cloudinaryService = cloudinaryService;
        _productRepository = productRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] Guid? categoryId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetProductsQuery(categoryId), ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Errors);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetProductByIdQuery(id), ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(result.Errors);
    }

    [HttpGet("by-name/{name}")]
    public async Task<IActionResult> GetByName(
        string name,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetProductByNameQuery(name), ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(result.Errors);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateProductCommand.CreateProductCommandRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateProductCommand(request), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById),
                new { id = result.Value.ProductId },
                result.Value)
            : BadRequest(result.Errors);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateProductCommand.UpdateProductCommandRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateProductCommand(id, request), ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Errors);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new DeleteProductCommand(id), ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(result.Errors);
    }


    [HttpGet("by-keyword")]
    public async Task<IActionResult> Search(
        [FromQuery] string? keyword,
        [FromQuery] Guid? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = new GetProductByKeywordQuery(
            keyword,
            categoryId,
            page,
            pageSize);

        var result = await _mediator.Send(query, ct);

        return Ok(result);
    }
    /// <summary>
    /// Upload image to Cloudinary (standalone, not bound to product)
    /// </summary>
    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(
        [FromForm] IFormFile imageFile,
        CancellationToken ct)
    {
        try
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { error = "File is required" });
            }

            var result = await _cloudinaryService.UploadImageAsync(imageFile);
            return Ok(new
            {
                url = result.Url,
                publicId = result.PublicId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Upload failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Upload image and assign to a specific product (overwrite existing image)
    /// </summary>
    [HttpPost("{productId:guid}/image")]
    public async Task<IActionResult> UploadProductImage(
        Guid productId,
        [FromForm] IFormFile imageFile,
        CancellationToken ct)
    {
        try
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { error = "File is required" });
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product is null)
            {
                return NotFound(new { error = $"Product with id '{productId}' not found" });
            }

            // Delete old image if exists
            if (!string.IsNullOrWhiteSpace(product.ImagePublicId))
            {
                await _cloudinaryService.DeleteImageAsync(product.ImagePublicId);
            }

            // Upload new image
            var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile);

            product.ImageUrl = uploadResult.Url;
            product.ImagePublicId = uploadResult.PublicId;

            await _productRepository.UpdateAsync(product);

            return Ok(new
            {
                productId = product.Id,
                imageUrl = product.ImageUrl,
                imagePublicId = product.ImagePublicId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Upload failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Delete image from Cloudinary by public ID
    /// Use catch-all route (**) to handle publicId with slashes (e.g., "products/xxx")
    /// </summary>
    [HttpDelete("images/{**publicId}")]
    public async Task<IActionResult> DeleteImage(
        string publicId,
        CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return BadRequest(new { error = "Public ID is required" });
            }

            await _cloudinaryService.DeleteImageAsync(publicId);
            return Ok(new { message = "Image deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Delete failed: {ex.Message}" });
        }
    }
}



