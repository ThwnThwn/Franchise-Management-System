using Grpc.Core;
using Grpc.Core.Interceptors;

namespace SupplierService.API.Interceptors;

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
        catch (Exception ex)
        {
            throw MapToRpcException(ex);
        }
    }

    private RpcException MapToRpcException(Exception exception)
    {
        return exception switch
        {
            // 1. Not Found
            KeyNotFoundException ex => new RpcException(new Status(StatusCode.NotFound, ex.Message)),

            // 2. Business Logic / Validation
            InvalidOperationException ex => new RpcException(new Status(StatusCode.InvalidArgument, ex.Message)),
            ArgumentException ex => new RpcException(new Status(StatusCode.InvalidArgument, ex.Message)),

            // 3. Security / Auth
            UnauthorizedAccessException ex => new RpcException(new Status(StatusCode.Unauthenticated, ex.Message)),

            // 4. Pass-through RpcException
            RpcException ex => ex,

            // 5. Default / Unhandled (log)
            _ => LogAndReturn(exception, StatusCode.Internal, "Internal Server Error")
        };
    }

    private RpcException LogAndReturn(Exception ex, StatusCode statusCode, string message)
    {
        _logger.LogError(ex, message);
        return new RpcException(new Status(statusCode, message));
    }
}
