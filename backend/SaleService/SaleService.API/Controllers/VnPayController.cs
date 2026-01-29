using Microsoft.AspNetCore.Mvc;
using MediatR;
using SalesService.Application.Features.Payments.VnPayReturn;
using SalesService.Application.Features.Payments.CreateVnPayUrl;

namespace SalesService.API.Controllers;

[ApiController]
[Route("api/vnpay")]
public class VnPayController : ControllerBase
{
    private readonly IMediator _mediator;

    public VnPayController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create VNPay payment URL for an order.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>

    [HttpPost("{id:guid}/payments/vnpay-url")]
    public async Task<ActionResult<CreateVnPayUrlResponse>> CreateVnPayUrl(
        Guid id,
        CancellationToken ct)
    {
        var command = new CreateVnPayUrlCommand(id);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            return NotFound(result.Errors);

        if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            return BadRequest(result.ValidationErrors);

        return StatusCode(500, result.Errors);
    }

    /// <summary>
    /// VNPay return URL (client redirect) + server-side verification.
    /// </summary>
    // Match: https://localhost:5003/api/vnpay/return
    [HttpGet("return")]
    public async Task<IActionResult> Return(CancellationToken ct)
    {
        var queryDict = HttpContext.Request.Query
            .ToDictionary(k => k.Key, v => v.Value.ToString());

        var cmd = new VnPayReturnCommand(queryDict);
        var result = await _mediator.Send(cmd, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Status switch
        {
            Ardalis.Result.ResultStatus.NotFound => NotFound(result.Errors),
            Ardalis.Result.ResultStatus.Invalid => BadRequest(result.ValidationErrors),
            _ => StatusCode(500, result.Errors)
        };
    }
}

