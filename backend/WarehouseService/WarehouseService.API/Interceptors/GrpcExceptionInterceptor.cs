using Grpc.Core;
using Grpc.Core.Interceptors;

namespace WarehouseService.API.Interceptors;

/// <summary>
/// Global gRPC interceptor — bắt tất cả exception, log chi tiết và trả về RpcException chuẩn.
/// </summary>
public class GrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionInterceptor> _logger;

    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("[gRPC] NotFound — {Method}: {Message}", context.Method, ex.Message);
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "[gRPC] InvalidOperation — {Method}: {Message}\nInner: {Inner}\nStack:\n{Stack}",
                context.Method,
                ex.Message,
                ex.InnerException?.Message ?? "none",
                ex.StackTrace);
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("[gRPC] Unauthorized — {Method}: {Message}", context.Method, ex.Message);
            throw new RpcException(new Status(StatusCode.PermissionDenied, ex.Message));
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = string.Join(" | ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            _logger.LogWarning("[gRPC] Validation — {Method}: {Errors}", context.Method, errors);
            throw new RpcException(new Status(StatusCode.InvalidArgument, errors));
        }
        catch (Exception ex)
        {
            // Unwrap toàn bộ inner exceptions
            var innerChain = BuildInnerExceptionChain(ex);

            _logger.LogError(
                "[gRPC] Unhandled — Method: {Method}\n" +
                "Type: {ExType}\n" +
                "Message: {Message}\n" +
                "InnerChain:\n{InnerChain}\n" +
                "StackTrace:\n{StackTrace}",
                context.Method,
                ex.GetType().Name,
                ex.Message,
                innerChain,
                ex.StackTrace);

            // Trả về full detail để dễ debug
            throw new RpcException(new Status(
                StatusCode.Internal,
                $"{ex.GetType().Name}: {ex.Message} | Inner: {ex.InnerException?.Message ?? "none"}"
            ));
        }
    }

    private static string BuildInnerExceptionChain(Exception ex)
    {
        var lines = new List<string>();
        var inner = ex.InnerException;
        var depth = 1;

        while (inner != null)
        {
            lines.Add($"  [{depth}] {inner.GetType().Name}: {inner.Message}");
            inner = inner.InnerException;
            depth++;
        }

        return lines.Count > 0 ? string.Join("\n", lines) : "none";
    }
}