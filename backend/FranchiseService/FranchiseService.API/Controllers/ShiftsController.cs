using BuildingBlocks.Core.Extensions;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.Commands.AssignStaff;
using FranchiseService.Application.Features.Shifts.Commands.CreateShift;
using FranchiseService.Application.Features.Shifts.Commands.DeleteShift;
using FranchiseService.Application.Features.Shifts.Commands.PublishShift;
using FranchiseService.Application.Features.Shifts.Commands.RemoveStaff;
using FranchiseService.Application.Features.Shifts.Commands.UpdateShift;
using FranchiseService.Application.Features.Shifts.DTOs;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Features.Shifts.Queries.GetShifts;
using FranchiseService.Application.Features.Shifts.Queries.GetShiftById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FranchiseService.API.Controllers
{
    /// <summary>
    /// Controller quản lý Ca làm việc (Shifts).
    /// Các endpoint khớp với FE shiftApi.ts.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ShiftsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShiftsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy danh sách ca làm việc (có phân trang, filter status).
        /// FE: shiftApi.getAll({ query: GetShiftsParams })
        /// Response: PageResult&lt;ShiftResponse&gt;
        /// </summary>
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

        /// <summary>
        /// Lấy chi tiết 1 ca làm việc kèm danh sách phân công.
        /// FE: shiftApi.getById({ id })
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetShiftById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetShiftByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Tạo ca làm việc mới (Draft).
        /// FE: shiftApi.create(req) → void
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateShift([FromBody] CreateShiftCommand command)
        {
            //command.CreatedBy = lấy từ JWT Token
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetShiftById), new { id }, new { Id = id });
        }

        /// <summary>
        /// Cập nhật ca làm việc (Partial Update).
        /// FE: shiftApi.update({ id, req }) → PATCH
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateShift(Guid id, [FromBody] UpdateShiftCommand command)
        {
            command.Id = id;
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Xóa ca làm việc (Soft Delete).
        /// FE: shiftApi.delete({ id }) → void
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(Guid id)
        {
            var command = new DeleteShiftCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Gán nhân viên vào ca làm việc.
        /// FE: shiftApi.assignStaff({ id, userIds }) → void
        /// </summary>
        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignStaff(Guid id, [FromBody] AssignStaffCommand command)
        {
            command.ShiftId = id;
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Xóa nhân viên khỏi ca làm việc.
        /// FE: shiftApi.removeStaff({ id, userId }) → void
        /// </summary>
        [HttpDelete("{id}/assign/{userId}")]
        public async Task<IActionResult> RemoveStaff(Guid id, Guid userId)
        {
            var command = new RemoveStaffCommand
            {
                ShiftId = id,
                UserId = userId
            };
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Publish ca làm việc (Draft → Published).
        /// FE: shiftApi.publish({ id }) → void
        /// </summary>
        [HttpPost("{id}/publish")]
        public async Task<IActionResult> PublishShift(Guid id)
        {
            var command = new PublishShiftCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
