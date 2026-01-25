using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.Products.GetById;
using ProductService.Application.Features.Products.GetList;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetProducts(
        [FromQuery] Guid? categoryId,
        CancellationToken ct)
    {
        var query = new GetProductsQuery(categoryId);
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Errors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetProductById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.Status == ResultStatus.NotFound)
            return NotFound(result.Errors);

        return BadRequest(result.Errors);
    }
}
