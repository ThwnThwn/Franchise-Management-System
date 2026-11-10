using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SalesService.API.Middleware;

public class GlobalExceptionHandlerMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (FluentValidation.ValidationException vEx) // FluentValidation
        {
            var errors = vEx.Errors.Select(e => e.ErrorMessage).ToList();
            var detail = string.Join("; ", errors);
            await WriteProblemDetailsAsync(context, 400, "Validation Failed", detail);
        }
        catch (ValidationException vEx) // DataAnnotations
        {
            await WriteProblemDetailsAsync(context, 400, "Validation Failed", vEx.Message);
        }
        catch (UnauthorizedAccessException uaeEx)
        {
            await WriteProblemDetailsAsync(context, 401, "Unauthorized", uaeEx.Message);
        }
        catch (KeyNotFoundException kEx)
        {
            await WriteProblemDetailsAsync(context, 404, "Not Found", kEx.Message);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database update error");
            await WriteProblemDetailsAsync(context, 409, "Database Error", dbEx.Message);
        }
        catch (OperationCanceledException oCEx)
        {
            await WriteProblemDetailsAsync(context, 408, "Request Timeout", oCEx.Message);
        }
        catch (NotImplementedException nNEEx)
        {
            await WriteProblemDetailsAsync(context, 501, "Not Implemented", nNEEx.Message);
        }
        catch (BadHttpRequestException badReq)
        {
            await WriteProblemDetailsAsync(context, 400, "Bad Request", badReq.Message);
        }
        catch (HttpRequestException httpEx)
        {
            await WriteProblemDetailsAsync(context, 502, "External Request Failed", httpEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemDetailsAsync(context, 500, "Internal Server Error", ex.Message);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new
        {
            title,
            status = statusCode,
            detail
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails), context.RequestAborted)
            .ConfigureAwait(false);
    }
}
