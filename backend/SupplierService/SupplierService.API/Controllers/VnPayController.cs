using Microsoft.AspNetCore.Mvc;
using MediatR;
using SupplierService.Application.Features.SupplierPayments.Commands.VnPayReturn;
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
    /// VNPay redirects here after the user completes payment.
    /// Verifies the signature, updates payment/PO status, then redirects the browser to the frontend.
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
                .ToDictionary(k => k.Key, v => v.Value.ToString());

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
}
