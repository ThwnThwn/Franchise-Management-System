using Microsoft.AspNetCore.Mvc;
using MediatR;
using SupplierService.Application.Features.SupplierPayments.Commands.VnPayReturn;
using SupplierService.Application.Features.SupplierPayments.Commands.VnPayIpn;
using Microsoft.Extensions.Logging;

namespace SupplierService.API.Controllers;

[ApiController]
[Route("api/vnpay")]
public class VnPayController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VnPayController> _logger;

    public VnPayController(IMediator mediator, ILogger<VnPayController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// VNPay return URL for supplier payments (purchase orders).
    /// VNPay redirects the browser here after the user completes payment.
    /// Only verifies the signature and redirects the browser to the frontend.
    /// Database updates are handled exclusively by the IPN endpoint.
    /// </summary>
    [HttpGet("return")]
    public async Task<IActionResult> Return(CancellationToken ct)
    {
        var clientUrl = HttpContext.RequestServices
            .GetService<IConfiguration>()
            ?.GetSection("VNPay")["SupplierClientReturnUrl"]
            ?? "http://localhost:3000/orders/payment/result";

        var queryString = HttpContext.Request.QueryString.Value ?? "";
        var separator = queryString.Contains('?') ? "&" : "?";

        try
        {
            var queryDict = HttpContext.Request.Query
                .ToDictionary(k => k.Key, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            var result = await _mediator.Send(new VnPayReturnCommand(queryDict), ct);

            string paymentStatus = "failed";
            if (result.IsSuccess && result.Value != null)
                paymentStatus = result.Value.Success ? "success" : "failed";

            var redirectUrl = $"{clientUrl}{queryString}{separator}payment_status={paymentStatus}";
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in VNPay return handler");
            return Redirect($"{clientUrl}{queryString}{separator}payment_status=failed");
        }
    }

    /// <summary>
    /// VNPay IPN (Instant Payment Notification) - Server-to-server callback.
    /// VNPay calls this endpoint directly (not via browser) to notify payment result.
    /// This is the authoritative source for updating payment and PO status in the database.
    /// Must return JSON: { "RspCode": "00", "Message": "Confirm Success" } on success.
    /// </summary>
    [HttpGet("ipn")]
    public async Task<IActionResult> Ipn(CancellationToken ct)
    {
        try
        {
            var queryDict = HttpContext.Request.Query
                .ToDictionary(k => k.Key, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            _logger.LogInformation("[VNPay IPN] Received IPN call with params: {Params}",
                string.Join(", ", queryDict.Select(kv => $"{kv.Key}={kv.Value}")));

            var cmd = new VnPayIpnCommand(queryDict);
            var result = await _mediator.Send(cmd, ct);

            if (result.IsSuccess && result.Value != null)
            {
                return Ok(result.Value);
            }

            return Ok(new { RspCode = "99", Message = "Unknown error" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VNPay IPN] Unhandled exception in IPN handler");
            return Ok(new { RspCode = "99", Message = "Unknown error" });
        }
    }
}
