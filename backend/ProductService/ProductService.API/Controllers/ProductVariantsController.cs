using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.ProductVariants.Queries.GetById;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/productVariants")]
public class ProductVariantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductVariantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductVariantByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result.Errors);
    }
}
