using BuildingBlocks.Core.Extensions;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.Commands.CreateShift;
using FranchiseService.Application.Features.Shifts.Commands.DeleteShift;
using FranchiseService.Application.Features.Shifts.Commands.PublishShift;
using FranchiseService.Application.Features.Shifts.Commands.UpdateShift;
using FranchiseService.Application.Features.Shifts.Commands.UpdateShiftAssignment;
using FranchiseService.Application.Features.Shifts.DTOs;
using FranchiseService.Application.Features.Shifts.Queries.GetAvailableStaffs;
using FranchiseService.Application.Features.Shifts.Queries.GetShifts;
using FranchiseService.Application.Features.Shifts.Queries.GetShiftById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FranchiseService.API.Controllers
{
    // Controller quan ly Ca lam viec (Shifts)
    // Cac endpoint khop voi FE shiftApi.ts
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ShiftsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShiftsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Lay danh sach ca lam viec (co phan trang, filter status)
        // FE: shiftApi.getAll({ query: GetShiftsParams })
        [HttpGet]
        public async Task<IActionResult> GetShifts(
            [FromQuery] Guid storeId,
            [FromQuery] DateOnly fromDate,
            [FromQuery] DateOnly toDate,
            [FromQuery] int? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetShiftsQuery(storeId, fromDate, toDate, page, pageSize, status);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        // Lay chi tiet 1 ca lam viec kem danh sach phan cong
        // FE: shiftApi.getById({ id })
        [HttpGet("{id}")]
        public async Task<IActionResult> GetShiftById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetShiftByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        // Tao ca lam viec moi (Draft)
        // FE: shiftApi.create(req)
        [HttpPost]
        public async Task<IActionResult> CreateShift([FromBody] CreateShiftCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetShiftById), new { id }, new { Id = id });
        }

        // Cap nhat ca lam viec (Partial Update)
        // FE: shiftApi.update({ id, req }) -> PATCH
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateShift(Guid id, [FromBody] UpdateShiftCommand command)
        {
            command.Id = id;
            await _mediator.Send(command);
            return NoContent();
        }

        // Xoa ca lam viec (Soft Delete)
        // FE: shiftApi.delete({ id })
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(Guid id)
        {
            var command = new DeleteShiftCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }

        // Dong bo danh sach nhan vien trong ca (Sync Logic)
        // FE: shiftApi.updateAssignments({ shiftId, userIds }) -> PUT
        // BE se tu diff: ai can them, ai can xoa, ai giu nguyen
        [HttpPut("{id}/assign")]
        public async Task<IActionResult> UpdateAssignments(Guid id, [FromBody] List<Guid> userIds)
        {
            var command = new UpdateShiftAssignmentCommand
            {
                ShiftId = id,
                UserIds = userIds
            };
            await _mediator.Send(command);
            return NoContent();
        }

        // Lay danh sach nhan vien kha dung cho ca lam (kem validation)
        // FE: shiftApi.getAvailableStaffs({ storeId, shiftId })
        // URL: GET /stores/{storeId}/shifts/{shiftId}/candidates
        [HttpGet("/api/v1/stores/{storeId}/shifts/{shiftId}/candidates")]
        public async Task<IActionResult> GetAvailableStaffs(Guid storeId, Guid shiftId)
        {
            var query = new GetAvailableStaffsQuery(storeId, shiftId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // Publish ca lam viec (Draft -> Published)
        // FE: shiftApi.publish({ id })
        [HttpPost("{id}/publish")]
        public async Task<IActionResult> PublishShift(Guid id)
        {
            var command = new PublishShiftCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}

