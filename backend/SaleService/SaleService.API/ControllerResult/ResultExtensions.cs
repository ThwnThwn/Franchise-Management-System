
    using Ardalis.Result;
    using Microsoft.AspNetCore.Mvc;

    namespace SalesService.API.ControllerResult;

    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            return result.Status switch
            {
                ResultStatus.Ok => new OkObjectResult(result.Value),

                ResultStatus.Invalid => new BadRequestObjectResult(new
                {
                    errors = result.ValidationErrors.Select(e => new
                    {
                        field = e.Identifier,
                        message = e.ErrorMessage
                    })
                }),

                ResultStatus.NotFound => new NotFoundObjectResult(new
                {
                    errors = result.Errors
                }),

                ResultStatus.Forbidden => new ObjectResult(new
                {
                    errors = result.Errors
                })
                { StatusCode = StatusCodes.Status403Forbidden },

                ResultStatus.Error => new ObjectResult(new
                {
                    errors = result.Errors
                })
                { StatusCode = StatusCodes.Status500InternalServerError },

                _ => new BadRequestObjectResult(new
                {
                    errors = result.Errors
                })
            };
        }
    }
