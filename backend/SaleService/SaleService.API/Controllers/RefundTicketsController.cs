using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesService.API.ControllerResult;
using SalesService.Application.Features.RefundTickets.Command.ApproveRefund;
using SalesService.Application.Features.RefundTickets.Command.CancelRefund;
using SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer;
using SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund;
using SalesService.Application.Features.RefundTickets.Command.RejectRefundByAdmin;
using SalesService.Application.Features.RefundTickets.Queries.GetAll;
using SalesService.Application.Features.RefundTickets.Queries.GetById;
using SalesService.Application.Features.RefundTickets.Queries.GetByStatus;
using SalesService.Application.Features.RefundTickets.Queries.GetByUserId;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund.InternalUserCreateRefundTicketCommand;

namespace SalesService.API.Controllers;

[ApiController]
[Route("api/refund-tickets")]
public class RefundTicketsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RefundTicketsController> _logger;

    public RefundTicketsController(IMediator mediator, ILogger<RefundTicketsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("customer")]
    public async Task<IActionResult> CustomerCreateRefund(
        [FromBody] CustomerCreateRefundRequestDto request,
        CancellationToken cancellationToken)
    {
        var customerId = GetUserIdFromClaims();
        if(request.Reason == null)
        {
            throw new ArgumentNullException(nameof(request.Reason));
        }
        var commandRequest = new CustomerCreateRefundTicketCommand.CustomerCreateRefundTicketRequest
        {
            Amount = request.Amount,
            Reason = request.Reason
        };

        var command = new CustomerCreateRefundTicketCommand(customerId, commandRequest);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new RefundTicketResponseDto
        {
            Id = result.Value.Id,
            TicketCode = result.Value.TicketCode,
            Amount = result.Value.Amount,
            Reason = result.Value.Reason,
            Status = result.Value.Status,
            CreatedAt = result.Value.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { refundTicketId = response.Id }, response);
    }

    [HttpPost("internal")]
    //[Authorize(Roles = "admin,manager,staff")]
    public async Task<IActionResult> InternalCreateRefund(
        [FromBody] InternalCreateRefundRequestDto request,
        CancellationToken cancellationToken)
    {
        var createdByUserId = GetUserIdFromClaims();

        var commandRequest = new InternalUserCreateRefundTicketRequest
        {
            //WalletId = request.WalletId,
            //OriginalTransactionId = request.OriginalTransactionId,
            Amount = request.Amount,
            Reason = request.Reason,
            Notes = request.InternalNotes
        };

        var overrideInfo = new Application.Common.Models.InternalUserOverrideInfo
        {
            AutoApprove = request.OverrideInfo?.AutoApprove ?? false,
            BypassAmountValidation = request.OverrideInfo?.BypassAmountValidation ?? false,
            BypassTimeLimit = request.OverrideInfo?.BypassTimeLimit ?? false,
            RefundReason = request.OverrideInfo?.RefundReason ?? RefundReasonType.NormalProcess
        };

        var command = new InternalUserCreateRefundTicketCommand(createdByUserId, commandRequest, overrideInfo);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new InternalRefundTicketResponseDto
        {
            Id = result.Value.Id,
            TicketCode = result.Value.TicketCode,
            Amount = result.Value.Amount,
            Reason = result.Value.Reason,
            //WalletId = result.Value.WalletId,
            TransactionId = result.Value.TransactionId,
            Status = result.Value.Status,
            CreatedAt = result.Value.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { refundTicketId = response.Id }, response);
    }

    [HttpPost("{refundTicketId}/approve")]
    //[Authorize(Roles = "admin")]
    public async Task<IActionResult> ApproveRefund(
        [FromRoute] Guid refundTicketId,
        [FromBody] ApproveRefundRequestDto request,
        CancellationToken cancellationToken)
    {
        var internalUserId = GetUserIdFromClaims();

        var requestData = new ApproveRefundCommand.ApproveRefundRequest
        {
            ApprovalNotes = request.ApprovalNotes,
            AutoComplete = request.AutoComplete
        };

        var command = new ApproveRefundCommand(refundTicketId, requestData);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new ApproveRefundResponseDto
        {
            RefundTicketId = result.Value.RefundTicketId,
            TicketCode = result.Value.TicketCode,
            Status = result.Value.Status,
            Amount = result.Value.Amount,
            CustomerId = result.Value.CustomerId,
            //ApprovedBy = result.Value.ApprovedBy,
            ApprovedAt = result.Value.ApprovedAt,
            ApprovalNotes = result.Value.ApprovalNotes,
            NextSteps = result.Value.NextSteps
        };

        return Ok(response);
    }

    [HttpPost("{refundTicketId}/reject")]
    //[Authorize(Roles = "admin")]
    public async Task<IActionResult> RejectRefund(
        [FromRoute] Guid refundTicketId,
        [FromBody] RejectRefundRequestDto request,
        CancellationToken cancellationToken)
    {
        var internalUserId = GetUserIdFromClaims();

        var requestData = new RejectRefundCommand.RejectRefundRequest
        {
            RejectNotes = request.RejectNotes
        };

        var command = new RejectRefundCommand(refundTicketId, requestData);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new RejectRefundResponseDto
        {
            RefundTicketId = result.Value.RefundTicketId,
            TicketCode = result.Value.TicketCode,
            Status = result.Value.Status,
            Amount = result.Value.Amount,
            CustomerId = result.Value.CustomerId,
            //RejectedBy = result.Value.RejectdBy,
            RejectedAt = result.Value.RejectdAt,
            RejectNotes = result.Value.RejectNotes,
            NextSteps = result.Value.NextSteps
        };

        return Ok(response);
    }

    [HttpPost("{refundTicketId}/cancel")]
    public async Task<IActionResult> CancelRefund(
        [FromRoute] Guid refundTicketId,
        [FromBody] CancelRefundRequestDto request,
        CancellationToken cancellationToken)
    {
        var customerId = GetUserIdFromClaims();

        var requestData = new CancelRefundCommand.CancelRefundRequest
        {
            CancellationReason = request.CancellationReason
        };

        var command = new CancelRefundCommand(refundTicketId, requestData);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new CancelRefundResponseDto
        {
            RefundTicketId = result.Value.RefundTicketId,
            TicketCode = result.Value.TicketCode,
            Status = result.Value.Status,
            Amount = result.Value.Amount,
            CancellationReason = result.Value.CancellationReason,
            CancelledAt = result.Value.CancelledAt,
            Message = result.Value.Message
        };

        return Ok(response);
    }

    [HttpGet("{refundTicketId}")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid refundTicketId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        var query = new GetRefundByIdQuery(refundTicketId, userId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.Status == ResultStatus.NotFound)
            return NotFound(new { error = "Refund ticket not found" });

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new RefundTicketDetailDto
        {
            Id = result.Value.Id,
            TicketCode = result.Value.TicketCode,
            Amount = result.Value.Amount,
            RefundAmount = result.Value.Amount,
            Reason = result.Value.Reason,
            RejectionReason = result.Value.Reason,
            WalletId = result.Value.WalletId,
            UserId = result.Value.UserId,
            TransactionId = result.Value.TransactionId,
            OriginalTransactionId = result.Value.OriginalTransactionId,
            Status = result.Value.Status,
            RefundReason = result.Value.RefundReason,
            RefundMethod = result.Value.RefundTicketMethod,
            CreatedAt = result.Value.CreatedAt,
            UpdatedAt = result.Value.UpdatedAt
        };

        return Ok(response);
    }

    //[HttpGet]
    //[Authorize(Roles = "admin")]
    //public async Task<IActionResult> GetAll(
    //    [FromQuery] int pageIndex = 1,
    //    [FromQuery] int pageSize = 20,
    //    CancellationToken cancellationToken = default)
    //{
    //    var internalUserId = GetUserIdFromClaims();
    //    var query = new GetAllRefundsQuery(UserId,, pageIndex, pageSize);
    //    var result = await _mediator.Send(query, cancellationToken);

    //    if (!result.IsSuccess)
    //        return BadRequest(new { errors = result.Errors });

    //    var response = new PagedRefundListResponseDto
    //    {
    //        Items = result.Value.Items.Select(MapToListItem).ToList(),
    //        PageIndex = result.Value.Page,
    //        PageSize = result.Value.PageSize,
    //        TotalCount = (int)result.Value.TotalCount,
    //        TotalPages = result.Value.TotalPages
    //    };

    //    return Ok(response);
    //}

    //[HttpGet("status/{status}")]
    //public async Task<IActionResult> GetByStatus(
    //    [FromRoute] RefundTicketStatus status,
    //    [FromQuery] int pageIndex = 1,
    //    [FromQuery] int pageSize = 20,
    //    CancellationToken cancellationToken = default)
    //{
    //    var userId = GetUserIdFromClaims();
    //    var query = new GetRefundByStatusQuery(userId, status, pageIndex, pageSize);
    //    var result = await _mediator.Send(query, cancellationToken);

    //    if (!result.IsSuccess)
    //        return result.ToActionResult();

    //    var response = new PagedRefundListResponseDto
    //    {
    //        Items = result.Value.Items.Select(MapToListItem).ToList(),
    //        PageIndex = result.Value.Page,
    //        PageSize = result.Value.PageSize,
    //        TotalCount = (int)result.Value.TotalCount,
    //        TotalPages = result.Value.TotalPages
    //    };

    //    return Ok(response);
    //}

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetByUserId(
        [FromRoute] Guid userId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var loggedUserId = GetUserIdFromClaims();
        var query = new GetRefundByUserIdQuery(userId, loggedUserId, pageIndex, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var response = new PagedRefundListResponseDto
        {
            Items = result.Value.Items.Select(MapToListItem).ToList(),
            PageIndex = result.Value.Page,
            PageSize = result.Value.PageSize,
            TotalCount = (int)result.Value.TotalCount,
            TotalPages = result.Value.TotalPages
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

    private static RefundListItemDto MapToListItem(dynamic refund)
    {
        return new RefundListItemDto
        {
            Id = refund.Id,
            TicketCode = refund.TicketCode,
            Amount = refund.Amount,
            RefundAmount = refund.RefundAmount,
            Reason = refund.Reason,
            WalletId = refund.WalletId,
            UserId = refund.UserId,
            TransactionId = refund.TransactionId,
            OriginalTransactionId = refund.OriginalTransactionId,
            Status = refund.Status,
            CreatedAt = refund.CreatedAt,
            UpdatedAt = refund.UpdatedAt
        };
    }
}

// ============== DTOs ==============

public record CustomerCreateRefundRequestDto
{
    public decimal Amount { get; init; }
    public string? Reason { get; init; }
}

public record InternalCreateRefundRequestDto
{
    public Guid WalletId { get; init; }
    public Guid OriginalTransactionId { get; init; }
    public decimal Amount { get; init; }
    public string? Reason { get; init; }
    public string? InternalNotes { get; init; }
    public InternalUserOverrideInfoDto? OverrideInfo { get; init; }
}

public record InternalUserOverrideInfoDto
{
    public bool AutoApprove { get; init; }
    public bool BypassAmountValidation { get; init; }
    public bool BypassTimeLimit { get; init; }
    public RefundReasonType RefundReason { get; init; }
}

public record ApproveRefundRequestDto
{
    public string? ApprovalNotes { get; init; }
    public bool AutoComplete { get; init; }
}

public record RejectRefundRequestDto
{
    public string? RejectNotes { get; init; }
}

public record CancelRefundRequestDto
{
    public string? CancellationReason { get; init; }
}

public record RefundTicketResponseDto
{
    public Guid Id { get; init; }
    public string TicketCode { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Reason { get; init; }
    public RefundTicketStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record InternalRefundTicketResponseDto
{
    public Guid Id { get; init; }
    public string TicketCode { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Reason { get; init; }
    public Guid WalletId { get; init; }
    public Guid? TransactionId { get; init; }
    public Guid OriginalTransactionId { get; init; }
    public RefundTicketStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record ApproveRefundResponseDto
{
    public Guid RefundTicketId { get; init; }
    public string TicketCode { get; init; } = string.Empty;
    public RefundTicketStatus Status { get; init; }
    public decimal Amount { get; init; }
    public Guid CustomerId { get; init; }
    public Guid ApprovedBy { get; init; }
    public DateTimeOffset ApprovedAt { get; init; }
    public string? ApprovalNotes { get; init; }
    public string? NextSteps { get; init; }
}

public record RejectRefundResponseDto
{
    public Guid RefundTicketId { get; init; }
    public string TicketCode { get; init; } = string.Empty;
    public RefundTicketStatus Status { get; init; }
    public decimal Amount { get; init; }
    public Guid CustomerId { get; init; }
    public Guid RejectedBy { get; init; }
    public DateTimeOffset RejectedAt { get; init; }
    public string? RejectNotes { get; init; }
    public string? NextSteps { get; init; }
}

public record CancelRefundResponseDto
{
    public Guid RefundTicketId { get; init; }
    public string TicketCode { get; init; } = string.Empty;
    public RefundTicketStatus Status { get; init; }
    public decimal Amount { get; init; }
    public string? CancellationReason { get; init; }
    public DateTimeOffset CancelledAt { get; init; }
    public string? Message { get; init; }
}

public record RefundTicketDetailDto
{
    public Guid Id { get; init; }
    public string TicketCode { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal RefundAmount { get; init; }
    public string? Reason { get; init; }
    public string? RejectionReason { get; init; }
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public Guid? TransactionId { get; init; }
    public Guid? OriginalTransactionId { get; init; }
    public RefundTicketStatus Status { get; init; }
    public RefundReasonType? RefundReason { get; init; }
    public RefundTicketMethod? RefundMethod { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record RefundListItemDto
{
    public Guid Id { get; init; }
    public string TicketCode { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal RefundAmount { get; init; }
    public string? Reason { get; init; }
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public Guid? TransactionId { get; init; }
    public Guid? OriginalTransactionId { get; init; }
    public RefundTicketStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record PagedRefundListResponseDto
{
    public List<RefundListItemDto> Items { get; init; } = new();
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}