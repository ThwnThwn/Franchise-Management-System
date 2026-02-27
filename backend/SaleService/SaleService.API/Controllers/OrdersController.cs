using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesService.API.ControllerResult;
using SalesService.Application.Features.Orders.Commands.Create;
using SalesService.Application.Features.Orders.Commands.PayWithWallet;
using SalesService.Application.Features.Orders.GetById;

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

    [HttpPost("{orderId}/pay-with-wallet")]
    public async Task<IActionResult> PayOrderWithWallet(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var customerId = GetUserIdFromClaims();

        var command = new PayOrderWithWalletCommand(orderId, customerId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.Status == ResultStatus.NotFound)
            return NotFound(new { error = result.Errors.FirstOrDefault() });

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new PayOrderWithWalletResponseDto
        {
            OrderId = result.Value.OrderId,
            OrderCode = result.Value.OrderCode,
            CustomerWalletId = result.Value.CustomerWalletId,
            ShopWalletId = result.Value.ShopWalletId,
            Amount = result.Value.Amount,
            CustomerBalanceBefore = result.Value.CustomerBalanceBefore,
            CustomerBalanceAfter = result.Value.CustomerBalanceAfter,
            ShopBalanceBefore = result.Value.ShopBalanceBefore,
            ShopBalanceAfter = result.Value.ShopBalanceAfter,
            CustomerTransactionId = result.Value.CustomerTransactionId,
            ShopTransactionId = result.Value.ShopTransactionId,
            CustomerTransactionCode = result.Value.CustomerTransactionCode,
            ShopTransactionCode = result.Value.ShopTransactionCode,
            PaidAt = result.Value.PaidAt
        };

        return Ok(response);
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Invalid user credentials");
    }
}

public record PayOrderWithWalletResponseDto
{
    public Guid OrderId { get; init; }
    public string OrderCode { get; init; } = string.Empty;
    public Guid CustomerWalletId { get; init; }
    public Guid ShopWalletId { get; init; }
    public decimal Amount { get; init; }
    public decimal CustomerBalanceBefore { get; init; }
    public decimal CustomerBalanceAfter { get; init; }
    public decimal ShopBalanceBefore { get; init; }
    public decimal ShopBalanceAfter { get; init; }
    public Guid CustomerTransactionId { get; init; }
    public Guid ShopTransactionId { get; init; }
    public string CustomerTransactionCode { get; init; } = string.Empty;
    public string ShopTransactionCode { get; init; } = string.Empty;
    public DateTimeOffset PaidAt { get; init; }
}
