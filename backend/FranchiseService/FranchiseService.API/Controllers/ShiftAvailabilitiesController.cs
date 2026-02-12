using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Features.ShiftAvailabilities.Commands.CreateStaffAvailability;
using FranchiseService.Application.Features.ShiftAvailabilities.Commands.DeleteStaffAvailability;
using FranchiseService.Application.Features.ShiftAvailabilities.Commands.UpdateStaffAvailability;
using FranchiseService.Application.Features.ShiftAvailabilities.DTOs;
using FranchiseService.Application.Features.ShiftAvailabilities.Queries.GetMyAvailability;
using FranchiseService.Application.Features.ShiftAvailabilities.Queries.GetStaffAvailabilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FranchiseService.API.Controllers;

[Route("api/v1/shift-availabilities")]
[ApiController]
public class ShiftAvailabilitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShiftAvailabilitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        // Thu lay x-user-id tu header
        if (Request.Headers.TryGetValue("x-user-id", out var userIdString) && 
            Guid.TryParse(userIdString, out var userId))
        {
            return userId;
        }

        // Fallback or throw unauthorized if strict
        // Tam thoi return empty guid hoac throw de bao loi ro rang
        throw new UnauthorizedAccessException("Missing x-user-id header");
    }

    // GET /api/v1/me/shift-availabilities
    // Staff xem lich su dang ky cua chinh minh
    [HttpGet("/api/v1/me/shift-availabilities")]
    public async Task<IActionResult> GetMyHistory([FromQuery] GetAvailabilityParams queryParams, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var query = new GetMyAvailabilityQuery
        {
            UserId = userId,
            Page = page,
            PageSize = pageSize,
            FromDate = queryParams.FromDate,
            ToDate = queryParams.ToDate,
            IsAvailable = queryParams.IsAvailable,
            IsRecurring = queryParams.IsRecurring,
            DayOfWeek = queryParams.DayOfWeek,
            Keyword = queryParams.Keyword
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // GET /api/v1/shift-availabilities
    // Manager xem toan bo
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAvailabilityParams queryParams, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetStaffAvailabilitiesQuery
        {
            Page = page,
            PageSize = pageSize,
            StoreId = queryParams.StoreId,
            UserId = queryParams.UserId,
            FromDate = queryParams.FromDate,
            ToDate = queryParams.ToDate,
            IsAvailable = queryParams.IsAvailable,
            IsRecurring = queryParams.IsRecurring,
            Keyword = queryParams.Keyword
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // POST /api/v1/shift-availabilities
    // Tao moi
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStaffAvailabilityRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new CreateStaffAvailabilityCommand(userId, request);
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetMyHistory), null, new { id });
    }

    // PATCH /api/v1/shift-availabilities/{id}
    // Cap nhat
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStaffAvailabilityRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateStaffAvailabilityCommand(id, userId, request);
        await _mediator.Send(command);
        return NoContent();
    }

    // DELETE /api/v1/shift-availabilities/{id}
    // Xoa
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        var command = new DeleteStaffAvailabilityCommand(id, userId);
        await _mediator.Send(command);
        return NoContent();
    }
}
