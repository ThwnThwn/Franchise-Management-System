using FranchiseService.Application.Features.ShiftTemplates.Commands.CreateTemplate;
using FranchiseService.Application.Features.ShiftTemplates.Commands.DeleteTemplate;
using FranchiseService.Application.Features.ShiftTemplates.Commands.UpdateTemplate;
using FranchiseService.Application.Features.ShiftTemplates.DTOs;
using FranchiseService.Application.Features.ShiftTemplates.Queries.GetShiftTemplateById;
using FranchiseService.Application.Features.ShiftTemplates.Queries.GetShiftTemplates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FranchiseService.API.Controllers
{
    /// <summary>
    /// Controller quản lý Mẫu ca làm việc (Shift Templates).
    /// Các endpoint khớp với FE shiftTemplateApi.ts.
    /// Route: /api/v1/shift-templates
    /// </summary>
    [Route("api/v1/shift-templates")]
    [ApiController]
    public class ShiftTemplatesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShiftTemplatesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy danh sách mẫu ca (filter theo storeId, isActive, keyword).
        /// FE: shiftTemplateApi.getAll({ query: GetShiftTemplatesParams }) → ShiftTemplateResponse[]
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? storeId,
            [FromQuery] bool? isActive,
            [FromQuery] string? keyword)
        {
            var query = new GetShiftTemplatesQuery(storeId, isActive, keyword);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết 1 mẫu ca.
        /// FE: shiftTemplateApi.getById({ id }) → ShiftTemplateResponse
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetShiftTemplateByIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Tạo mẫu ca mới.
        /// FE: shiftTemplateApi.create(req) → void
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTemplateCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, new { Id = id });
        }

        /// <summary>
        /// Cập nhật mẫu ca (Partial Update / PATCH).
        /// FE: shiftTemplateApi.update({ id, req }) → void
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateCommand command)
        {
            command.Id = id;
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Xóa mẫu ca (Soft Delete).
        /// FE: shiftTemplateApi.delete({ id }) → void
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteTemplateCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
