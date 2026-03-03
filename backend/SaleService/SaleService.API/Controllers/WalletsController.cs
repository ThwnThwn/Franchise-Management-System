using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesService.API.ControllerResult;
using SalesService.Application.Features.Wallets.Command.AddBalance;
using SalesService.Application.Features.Wallets.Command.Create;
using SalesService.Application.Features.Wallets.Command.DeductBalance;
using SalesService.Application.Features.Wallets.Commands.UpdateStatus;
using SalesService.Application.Features.Wallets.Deposit;
using SalesService.Application.Features.Wallets.Queries.GetByUserId;
using SalesService.Application.Features.Wallets.Withdraw;
using SalesService.Domain.Enums;

namespace SalesService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WalletController> _logger;

    public WalletController(
        IMediator mediator,
        ILogger<WalletController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // ================= CREATE WALLET =================

    [HttpPost]
    public async Task<IActionResult> CreateWallet(
        [FromBody] CreateWalletRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateWalletCommand(request.UserId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new CreateWalletResponseDto
        {
            WalletId = result.Value.WalletId,
            UserId = result.Value.UserId,
            Balance = result.Value.Balance,
            Status = result.Value.Status,
            CreatedAt = result.Value.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetWalletByUserId),
            new { userId = response.UserId },
            response);
    }

    // ================= ADD BALANCE =================

    [HttpPost("{userId}/add-balance")]
    public async Task<IActionResult> AddBalance(
        [FromRoute] Guid userId,
        [FromBody] AddBalanceRequestDto request,
        CancellationToken cancellationToken)
    {
        var walletQuery = new GetWalletByUserIdQuery(userId);
        var walletResult = await _mediator.Send(walletQuery, cancellationToken);

        if (!walletResult.IsSuccess)
            return walletResult.ToActionResult();

        var wallet = walletResult.Value;

        var command = new AddBalanceCommand(
            wallet.Id,
            userId,
            new AddBalanceCommand.AddBalanceCommandRequest
            {
                Amount = request.Amount,
                RowVersion = wallet.RowVersion,
                Description = request.Description,
                PaymentMethod = request.PaymentMethod,
                ExternalTransactionId = request.ExternalTransactionId
            });

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new AddBalanceResponseDto
        {
            WalletId = result.Value.WalletId,
            UserId = result.Value.UserId,
            BalanceBefore = result.Value.BalanceBefore,
            BalanceAfter = result.Value.BalanceAfter,
            AmountAdded = result.Value.AmountAdded,
            TransactionId = result.Value.TransactionId,
            TransactionCode = result.Value.TransactionCode,
            UpdatedAt = result.Value.UpdatedAt
        };

        return Ok(response);
    }

    // ================= DEDUCT BALANCE =================

    [HttpPost("{userId}/deduct-balance")]
    public async Task<IActionResult> DeductBalance(
        [FromRoute] Guid userId,
        [FromBody] DeductBalanceRequestDto request,
        CancellationToken cancellationToken)
    {
        var walletQuery = new GetWalletByUserIdQuery(userId);
        var walletResult = await _mediator.Send(walletQuery, cancellationToken);

        if (!walletResult.IsSuccess)
            return walletResult.ToActionResult();

        var wallet = walletResult.Value;

        var command = new DeductBalanceCommand(
            wallet.Id,
            userId,
            new DeductBalanceCommand.DeductBalanceCommandRequest
            {
                Amount = request.Amount,
                RowVersion = wallet.RowVersion,
                Description = request.Description,
                PaymentMethod = request.PaymentMethod,
                ExternalTransactionId = request.ExternalTransactionId
            });

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new DeductBalanceResponseDto
        {
            WalletId = result.Value.WalletId,
            UserId = result.Value.UserId,
            BalanceBefore = result.Value.BalanceBefore,
            BalanceAfter = result.Value.BalanceAfter,
            AmountDeducted = result.Value.AmountDeducted,
            TransactionId = result.Value.TransactionId,
            UpdatedAt = result.Value.UpdatedAt
        };

        return Ok(response);
    }

    // ================= UPDATE STATUS =================

    [HttpPatch("{walletId}/status")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> UpdateWalletStatus(
    [FromRoute] Guid walletId,
    [FromBody] UpdateWalletStatusRequestDto request,
    CancellationToken cancellationToken)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(new { errors = new[] { "Invalid admin credentials" } });

        var command = new UpdateWalletStatusCommand(
            walletId,
            adminId,
            new UpdateWalletStatusCommand.UpdateWalletStatusRequest
            {
                NewStatus = request.NewStatus,
                Reason = request.Reason,
                AdminNotes = request.AdminNotes
            });

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok(new UpdateWalletStatusResponseDto
        {
            WalletId = result.Value.WalletId,
            OldStatus = result.Value.OldStatus,
            NewStatus = result.Value.NewStatus,
            Reason = result.Value.Reason,
            UpdatedBy = result.Value.UpdatedBy,
            UpdatedAt = result.Value.UpdatedAt
        });
    }

    // ================= GET WALLET =================

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetWalletByUserId(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetWalletByUserIdQuery(userId);

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new GetWalletResponseDto
        {
            Id = result.Value.Id,
            UserId = result.Value.UserId,
            Balance = result.Value.Balance,
            Status = (WalletStatus)result.Value.Status,
            CreatedAt = result.Value.CreatedAt,
            UpdatedAt = result.Value.UpdatedAt
        };

        return Ok(response);
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(
            [FromBody] DepositWalletReq request,
            CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var command = new DepositWalletCommand(userId.Value, request);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ValidationErrors.Any())
            return BadRequest(result.ValidationErrors);

        return StatusCode(500, result.Errors);
    }

    [AllowAnonymous]
    [HttpGet("deposit/vnpay-return")]
    public async Task<IActionResult> DepositVnPayReturn(
    [FromQuery] IDictionary<string, string> queryParams,
    CancellationToken cancellationToken)
    {
        var command = new DepositVnPayReturnCommand(queryParams);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            if (queryParams.TryGetValue("redirectTo", out var redirectTo)
                && !string.IsNullOrEmpty(redirectTo)
                && Uri.TryCreate(redirectTo, UriKind.Absolute, out _)) // ✅ validate URL
            {
                var redirectUrl = $"{Uri.UnescapeDataString(redirectTo)}" +
                                  $"?success={result.Value.Success}" +
                                  $"&amount={result.Value.Amount}" +
                                  $"&newBalance={result.Value.NewBalance}";
                return Redirect(redirectUrl);
            }

            return Ok(result.Value);
        }

        return StatusCode(500, result.Errors);
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw(
        [FromBody] WithdrawWalletReq request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var command = new WithdrawWalletCommand(userId.Value, request);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ValidationErrors.Any())
            return BadRequest(result.ValidationErrors);

        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            return NotFound(result.Errors);

        if (result.Status == Ardalis.Result.ResultStatus.Forbidden)
            return Forbid();

        return StatusCode(500, result.Errors);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub");

        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
public record CreateWalletRequestDto
{
    public Guid UserId { get; init; }
}

public record CreateWalletResponseDto
{
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public decimal Balance { get; init; }
    public WalletStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record AddBalanceRequestDto
{
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public string? ExternalTransactionId { get; init; }
}

public record AddBalanceResponseDto
{
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public decimal BalanceBefore { get; init; }
    public decimal BalanceAfter { get; init; }
    public decimal AmountAdded { get; init; }
    public Guid TransactionId { get; init; }
    public string? TransactionCode { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record DeductBalanceRequestDto
{
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public string? ExternalTransactionId { get; init; }
}

public record DeductBalanceResponseDto
{
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public decimal BalanceBefore { get; init; }
    public decimal BalanceAfter { get; init; }
    public decimal AmountDeducted { get; init; }
    public Guid TransactionId { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record UpdateWalletStatusRequestDto
{
    public WalletStatus NewStatus { get; init; }
    public string? Reason { get; init; }
    public string? AdminNotes { get; init; }
}

public record UpdateWalletStatusResponseDto
{
    public Guid WalletId { get; init; }
    public WalletStatus OldStatus { get; init; }
    public WalletStatus NewStatus { get; init; }
    public string? Reason { get; init; }
    public Guid UpdatedBy { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record GetWalletResponseDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public decimal Balance { get; init; }
    public WalletStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
