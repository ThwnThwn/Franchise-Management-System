using Microsoft.AspNetCore.Mvc;
using MediatR;
using SalesService.Application.Features.Payments.VnPayReturn;
using SalesService.Application.Features.Payments.CreatePayment;
using SalesService.Application.Features.Payments.VnPayIpn;

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
    // Match: http://localhost:5003/api/vnpay/return
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

    /// <summary>
    /// VNPay IPN (Instant Payment Notification) - Server to Server callback.
    /// VNPay will call this endpoint to update payment status.
    /// </summary>
    // Match: https://<domain>/api/vnpay/ipn
    [HttpGet("ipn")]
    public async Task<IActionResult> Ipn(CancellationToken ct)
    {
        var orderInfo = HttpContext.Request.Query["vnp_OrderInfo"].ToString();

        // -------------------------------------------------------------------------
        // WORKAROUND DÙNG CHUNG TMNCODE: 
        // Phân luồng request về SupplierService (vì IPN từ VNPay cấu hình chung 1 URL)
        // -------------------------------------------------------------------------
        if (orderInfo.Contains("don nhap hang", StringComparison.OrdinalIgnoreCase) || 
            orderInfo.Contains("PO-", StringComparison.OrdinalIgnoreCase))
        {
            var queryString = HttpContext.Request.QueryString.Value;
            
            // Đọc cấu hình từ .env (SUPPLIER_REST_URL -> http://localhost:5023)
            var config = HttpContext.RequestServices.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            var supplierRestUrl = config?["SUPPLIER_REST_URL"] ?? "http://localhost:5023";
            
            using var httpClient = new HttpClient();
            try 
            {
                var response = await httpClient.GetAsync($"{supplierRestUrl}/api/vnpay/ipn{queryString}", ct);
                var content = await response.Content.ReadAsStringAsync(ct);
                return Content(content, "application/json"); // Trả về JSON của SupplierService cho VNPay
            }
            catch (Exception ex)
            {
                return Ok(new { RspCode = "99", Message = "Forward to SupplierService failed: " + ex.Message });
            }
        }

        var queryDict = HttpContext.Request.Query
            .ToDictionary(k => k.Key, v => v.Value.ToString());

        var cmd = new VnPayIpnCommand(queryDict);
        var result = await _mediator.Send(cmd, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value); // Trả về JSON: { "RspCode": "...", "Message": "..." }
        }

        // Nếu lỗi do logic của ta (ví dụ không lấy được config), vẫn phải trả về format JSON của VNPay.
        // Tuy nhiên theo chuẩn VNPay, ta nên handle tất cả trong Handler và luôn Success với RspCode phù hợp.
        // Đoạn này dự phòng:
        return Ok(new { RspCode = "99", Message = "Unknown error" });
    }
}

