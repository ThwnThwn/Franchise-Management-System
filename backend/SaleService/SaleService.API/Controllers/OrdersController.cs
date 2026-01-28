using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesService.Application.Features.Orders.GetById;
using SalesService.Application.Features.Orders.Commands.Create;

namespace SalesService.API.Controllers;

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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetOrderByIdResponse>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            return NotFound(result.Errors);

        if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            return BadRequest(result.ValidationErrors);

        return StatusCode(500, result.Errors);
    }
}