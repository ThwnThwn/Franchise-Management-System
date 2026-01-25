using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.Products.GetById;

namespace ProductService.API.Controllers.Products.GetById;

[ApiController]
[Route("api/products")]
public class GetProductByIdController : ControllerBase
{
    private readonly IMediator _mediator;

    public GetProductByIdController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<GetProductDetailDto>> GetById(
        [FromRoute] Guid productId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(productId), ct);

        if (result.IsSuccess)
            return Ok(result.Value);
        
        if (result.Status == ResultStatus.NotFound)
            return NotFound(result.Errors);

        return BadRequest(result.Errors);
    }
}
