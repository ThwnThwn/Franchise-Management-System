using Microsoft.AspNetCore.Mvc;
using MediatR;
using SalesService.Application.Features.Payments.VnPayReturn;
using SalesService.Application.Features.Payments.CreatePayment;

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
    public async Task<ActionResult<PaymentResponse>> CreateVnPayUrl(
        Guid id,
        CancellationToken ct)
    {
        var command = new CreatePaymentCommand
            (
            new CreatePaymentReq
            {
                OrderId = id,
                PaymentMethod = Domain.Enums.PaymentMethod.VNPay
            }
            );
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

        // Get client return URL from configuration
        var clientUrl = HttpContext.RequestServices
            .GetService<IConfiguration>()?
            .GetSection("VNPay")["ClientReturnUrl"];
            
        var targetUrl = clientUrl ?? "http://localhost:3000/payment/result";

        // Forward all VNPay query params to client URL
        var queryString = HttpContext.Request.QueryString.Value ?? "";
        
        // Determine payment status
        string paymentStatus = "failed";
        if (result.IsSuccess && result.Value != null)
        {
            paymentStatus = result.Value.Success ? "success" : "failed";
        }

        // Append payment_status to query string
        var separator = queryString.Contains("?") ? "&" : "?";
        var redirectUrl = $"{targetUrl}{queryString}{separator}payment_status={paymentStatus}";

        return Redirect(redirectUrl);
    }
}

