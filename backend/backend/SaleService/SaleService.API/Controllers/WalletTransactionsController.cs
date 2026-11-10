using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesService.API.ControllerResult;
using SalesService.Application.Features.WalletTransactions.Commands.CreateTransaction;
using SalesService.Application.Features.WalletTransactions.Queries.GetAll;
using SalesService.Application.Features.WalletTransactions.Queries.GetByCode;
using SalesService.Application.Features.WalletTransactions.Queries.GetById;
using SalesService.Application.Features.WalletTransactions.Queries.GetByUserId;
using SalesService.Application.Features.WalletTransactions.Querries.GetByOrderId;
using SalesService.Domain.Enums;

namespace SalesService.API.Controllers;

[ApiController]
[Route("api/wallet-transactions")]
[Authorize]
public class WalletTransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WalletTransactionsController> _logger;

    public WalletTransactionsController(
        IMediator mediator,
        ILogger<WalletTransactionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction(
        [FromBody] CreateTransactionRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();

        var innerRequest = new CreateTransactionCommand.CreateTransactionRequest
        {
            WalletId = request.WalletId,
            Amount = request.Amount,
            TransactionType = request.TransactionType,
            PaymentMethod = request.PaymentMethod,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            Description = request.Description,
            Notes = request.Notes,
            ExternalTransactionId = request.ExternalTransactionId,
            DestinationWalletId = request.DestinationWalletId,
            Metadata = request.Metadata
        };

        var command = new CreateTransactionCommand(
            userId,
            innerRequest,
            request.AdminOverride != null
                ? new CreateTransactionCommand.HasAdminOverride
                {
                    AdminId = request.AdminOverride.AdminId,
                    AutoComplete = request.AdminOverride.AutoComplete,
                    BypassBalanceCheck = request.AdminOverride.BypassBalanceCheck,
                    BypassTransactionLimits = request.AdminOverride.BypassTransactionLimits,
                    InternalNotes = request.AdminOverride.InternalNotes
                }
                : null);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        var response = new CreateTransactionResponseDto
        {
            Id = result.Value.Id,
            TransactionCode = result.Value.TransactionCode,
            WalletId = result.Value.WalletId,
            Amount = result.Value.Amount,
            TransactionType = result.Value.TransactionType,
            Status = result.Value.Status,
            BalanceBefore = result.Value.BalanceBefore,
            BalanceAfter = result.Value.BalanceAfter,
            ReferenceId = result.Value.ReferenceId,
            ReferenceType = result.Value.ReferenceType,
            PaymentMethod = result.Value.PaymentMethod,
            CreatedAt = result.Value.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { transactionId = response.Id }, response);
    }
    
    [HttpGet]
    [Authorize(Roles = "admin,manager" )]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var loggedUserId = GetUserIdFromClaims();
        var query = new GetAllWalletTransactionsQuery(loggedUserId, pageIndex, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new PagedTransactionListResponseDto
        {
            Items = result.Value.Items.Select(MapToDto).ToList(),
            PageIndex = result.Value.Page,
            PageSize = result.Value.PageSize,
            TotalCount = (int)result.Value.TotalCount,
            TotalPages = result.Value.TotalPages
        };

        return Ok(response);
    }

    [HttpGet("{transactionId}")]
    [Authorize(Roles = "admin,manager,staff")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid transactionId,
        CancellationToken cancellationToken)
    {

        var loggedUserId = GetUserIdFromClaims();
        var query = new GetWalletTransactionByIdQuery(transactionId, loggedUserId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.Status == ResultStatus.NotFound)
            return NotFound(new { error = "Transaction not found" });

        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok(MapToDto(result.Value));
    }

    [HttpGet("users/{userId}")]
    [Authorize(Roles = "admin,manager,staff")]
    public async Task<IActionResult> GetByUserId(
        [FromRoute] Guid userId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var loggedUserId = GetUserIdFromClaims();
        var query = new GetWalletTransactionsByUserIdQuery(userId, loggedUserId, pageIndex, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new PagedTransactionListResponseDto
        {
            Items = result.Value.Items.Select(MapToDto).ToList(),
            PageIndex = result.Value.Page,
            PageSize = result.Value.PageSize,
            TotalCount = (int)result.Value.TotalCount,
            TotalPages = result.Value.TotalPages
        };

        return Ok(response);
    }

    [HttpGet("code/{transactionCode}")]
    [Authorize(Roles = "admin,manager,staff")]
    public async Task<IActionResult> GetByCode(
        [FromRoute] string transactionCode,
        CancellationToken cancellationToken)
    {
        var loggedUserId = GetUserIdFromClaims();
        var query = new GetWalletTransactionByCodeQuery(transactionCode, loggedUserId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.Status == ResultStatus.NotFound)
            return NotFound(new { error = "Transaction not found" });
        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok(MapToDto(result.Value));
    }

    [HttpGet("orders/{orderId}")]
    [Authorize(Roles = "admin,manager,staff")]
    public async Task<IActionResult> GetByOrderId(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var loggedUserId = GetUserIdFromClaims();
        var query = new GetWalletTransactionByOrderIdQuery(orderId, loggedUserId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = result.Value.Select(MapToDto).ToList();
        return Ok(response);
    }

    [Authorize]
    [HttpGet("debug-claims")]
    public IActionResult DebugClaims()
    {
        return Ok(User.Claims.Select(c => new
        {
            c.Type,
            c.Value
        }));
    }
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Invalid user credentials");
    }

    private static TransactionDetailDto MapToDto(dynamic transaction)
    {
        return new TransactionDetailDto
        {
            Id = transaction.Id,
            WalletId = transaction.WalletId,
            UserId = transaction.UserId,
            OrderId = transaction.OrderId,
            InitiatedBy = transaction.InitiatedBy,
            ProcessedBy = transaction.ProcessedBy,
            ParentTransactionId = transaction.ParentTransactionId,
            TransactionCode = transaction.TransactionCode,
            Notes = transaction.Notes,
            Amount = transaction.Amount,
            BalanceBefore = transaction.BalanceBefore,
            BalanceAfter = transaction.BalanceAfter,
            TransactionType = transaction.TransactionType,
            Status = transaction.Status,
            ReferenceId = transaction.ReferenceId,
            ReferenceType = transaction.ReferenceType,
            Description = transaction.Description,
            ExternalTransactionId = transaction.ExternalTransactionId,
            Metadata = transaction.Metadata,
            PaymentMethod = transaction.PaymentMethod,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
            CompletedAt = transaction.CompletedAt,
            FailedAt = transaction.FailedAt,
            ProcessedAt = transaction.ProcessedAt,
            FailureReason = transaction.FailureReason
        };
    }
}

// ============== DTOs ==============

public record CreateTransactionRequestDto
{
    public Guid WalletId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType TransactionType { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public Guid? ReferenceId { get; init; }
    public WalletReferenceType? ReferenceType { get; init; }
    public string? Description { get; init; }
    public string? Notes { get; init; }
    public string? ExternalTransactionId { get; init; }
    public Guid? DestinationWalletId { get; init; }
    public string? Metadata { get; init; }
    public AdminOverrideDto? AdminOverride { get; init; }
}

public record AdminOverrideDto
{
    public Guid AdminId { get; init; }
    public bool AutoComplete { get; init; }
    public bool BypassBalanceCheck { get; init; }
    public bool BypassTransactionLimits { get; init; }
    public string? InternalNotes { get; init; }
}

public record CreateTransactionResponseDto
{
    public Guid Id { get; init; }
    public string TransactionCode { get; init; } = string.Empty;
    public Guid WalletId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType TransactionType { get; init; }
    public TransactionStatus Status { get; init; }
    public decimal BalanceBefore { get; init; }
    public decimal BalanceAfter { get; init; }
    public Guid? ReferenceId { get; init; }
    public WalletReferenceType? ReferenceType { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record TransactionDetailDto
{
    public Guid Id { get; init; }
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public Guid? OrderId { get; init; }
    public Guid? InitiatedBy { get; init; }
    public Guid? ProcessedBy { get; init; }
    public Guid? ParentTransactionId { get; init; }
    public string? TransactionCode { get; init; }
    public string? Notes { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceBefore { get; init; }
    public decimal BalanceAfter { get; init; }
    public TransactionType TransactionType { get; init; }
    public TransactionStatus Status { get; init; }
    public Guid? ReferenceId { get; init; }
    public WalletReferenceType? ReferenceType { get; init; }
    public string? Description { get; init; }
    public string? ExternalTransactionId { get; init; }
    public string? Metadata { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public DateTimeOffset? FailedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; init; }
    public string? FailureReason { get; init; }
}

public record PagedTransactionListResponseDto
{
    public List<TransactionDetailDto> Items { get; init; } = new();
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}