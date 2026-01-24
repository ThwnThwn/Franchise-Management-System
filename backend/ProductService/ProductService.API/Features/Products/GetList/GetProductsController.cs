using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.Products.GetList;

namespace ProductService.API.Features.Products.GetList;

[ApiController]
[Route("api/products")]
public class GetProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GetProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetProductsResultDto>> GetProducts(
        [FromQuery] Guid? categoryId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductsQuery(categoryId), ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Errors);
    }
}
