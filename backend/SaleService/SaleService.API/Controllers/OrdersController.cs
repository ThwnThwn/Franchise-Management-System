using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaleService.Application.Features.Orders.Create;

namespace SaleService.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CreateOrderResponseDto>> CreateOrder(
        [FromBody] CreateOrderRequestDto request,
        CancellationToken ct)
    {
        var command = new CreateOrderCommand(request);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Errors);
    }
}
