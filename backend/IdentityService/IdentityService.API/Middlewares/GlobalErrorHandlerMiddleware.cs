using IdentityService.Application.AppExceptions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace IdentityService.API.Middlewares
{
    public class GlobalErrorHandlerMiddleware : IMiddleware
    {
        public readonly ILogger<GlobalErrorHandlerMiddleware> _logger;
        public GlobalErrorHandlerMiddleware(ILogger<GlobalErrorHandlerMiddleware> logger)
        {
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (BusinessException bEx)
            {
                await WriteProblemDetailsAsync(context, 422, "Business Exception", bEx.Message);
            }
            catch (ForbidenException fEx)
            {
                await WriteProblemDetailsAsync(context, 403, "Not Have Permission", fEx.Message);
            }
            catch (ValidationException vEx) // System.ComponentModel.DataAnnotations
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
            catch (ConflictDuplicateException cDEx)
            {
                await WriteProblemDetailsAsync(context, 409, "Conflict", cDEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteProblemDetailsAsync(context, 500, "Internal Server Error", ex.Message);
            }
        }

        public static async Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string title, string detail)
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
            //await để chờ hành động hoàn thành trc khi chạy middle tiếp
            //Respone.WriteAsync sẽ ghi dữ liệu vào body của respone
            //JsonSerializer.Serialize(problemDetails) biến pronlemDetails thành json
            //context.RequestAborted nó là cancellationToken giúp huỷ tác vụ ghi respone nếu client mất kết nối
            //ConfigureAwait(false) cho phép thread khác có thể xử lí luồng này khi nó xog chứ k nhất thiết phải là thread lúc nãy
            //tránh deadlock
        }

    }
}
