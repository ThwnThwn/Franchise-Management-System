using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.Categories.Command.CreateCategory;
using ProductService.Application.Features.Categories.Command.DeleteCategory;
using ProductService.Application.Features.Categories.Command.UpdateCategory;
using ProductService.Application.Features.Categories.Queries.GetByCategoryName;
using ProductService.Application.Features.Categories.Queries.GetById;
using ProductService.Application.Features.Categories.Queries.GetList;
using System.Runtime.InteropServices;
using static ProductService.Application.Features.Categories.Command.CreateCategory.CreateCategoryCommand;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        public readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories(CancellationToken ct)
        {
            var query = new GetCategoriesQuery();
            var result = await _mediator.Send(query, ct);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Errors);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id,CancellationToken cancellationToken)
        {
            var result= await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Errors);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryCommandRequest request,CancellationToken cancellationToken)
        {
            var command = new CreateCategoryCommand(request);
            var result= await _mediator.Send(command,cancellationToken);
            return result.IsSuccess ?
                CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value) :
                BadRequest(result.Errors);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id,UpdateCategoryCommandRequest request,CancellationToken cancellationToken)
        {
            var command = new UpdateCategoryCommand(id, request);
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id,CancellationToken cancellationToken)
        {
            var command = new DeleteCategoryCommand(id);
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Errors);
        }

        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByCategoryName(string name,CancellationToken cancellationToken)
        {
            var query = new GetCategoryByNameQuery(name);
            var result = await _mediator.Send(query, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Errors);
        }
    }
}
