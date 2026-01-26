using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.Categories.GetList;

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
    }
}
