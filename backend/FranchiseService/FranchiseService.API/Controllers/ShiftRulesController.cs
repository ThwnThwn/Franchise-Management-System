using FranchiseService.Application.Features.ShiftRules.Commands.SaveShiftRule;
using FranchiseService.Application.Features.ShiftRules.DTOs;
using FranchiseService.Application.Features.ShiftRules.Queries.GetShiftRuleSettings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FranchiseService.API.Controllers;

[Route("api/v1/shift-rules")]
[ApiController]
public class ShiftRulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShiftRulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        if (Request.Headers.TryGetValue("x-user-id", out var userIdString) && 
            Guid.TryParse(userIdString, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("Missing x-user-id header");
    }

    // GET /api/v1/shift-rules?storeId=...
    [HttpGet]
    public async Task<IActionResult> GetSettings([FromQuery] Guid? storeId)
    {
        var query = new GetShiftRuleSettingsQuery { StoreId = storeId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // PUT /api/v1/shift-rules
    [HttpPut]
    public async Task<IActionResult> Save([FromBody] SaveShiftRuleRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new SaveShiftRuleCommand(userId, request);
        await _mediator.Send(command);
        return NoContent();
    }
}
