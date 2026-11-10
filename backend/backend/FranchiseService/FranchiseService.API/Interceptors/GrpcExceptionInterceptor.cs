using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace FranchiseService.API.Interceptors;

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
            // Bắt tất cả Exception và xử lý tại một hàm duy nhất
            throw MapToRpcException(ex);
        }
    }

    private RpcException MapToRpcException(Exception exception)
    {
        return exception switch
        {
            // 1. Validation (FluentValidation)
            ValidationException ex => new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(" | ", ex.Errors.Select(x => x.ErrorMessage)))),

            // 2. Logic Business / Not Found
            KeyNotFoundException ex => new RpcException(new Status(StatusCode.NotFound, ex.Message)),
            DirectoryNotFoundException ex => new RpcException(new Status(StatusCode.NotFound, ex.Message)),
            //BusinessException ex => new RpcException(new Status(StatusCode.InvalidArgument, ex.Message)),
            //ConflictDuplicateException ex => new RpcException(new Status(StatusCode.AlreadyExists, ex.Message)),
            InvalidOperationException ex => new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message)),

            // 3. Security / Auth
            //ForbidenException ex => new RpcException(new Status(StatusCode.PermissionDenied, ex.Message)),
            UnauthorizedAccessException ex => new RpcException(new Status(StatusCode.Unauthenticated, ex.Message)),

            // 4. Database Error (Có log)
            // 4. Database Error (Có log)
            DbUpdateException ex => LogAndReturn(ex, StatusCode.FailedPrecondition, $"Database Error: {ex.InnerException?.Message ?? ex.Message}"),

            // 5. Nếu đã là RpcException thì ném tiếp (pass-through)
            RpcException ex => ex,

            // 6. Default / Unhandled (Có log)
            _ => LogAndReturn(exception, StatusCode.Internal, $"Internal Server Error: {exception.Message}")
        };
    }

    // Helper function để vừa Log vừa trả về Exception trong switch expression
    private RpcException LogAndReturn(Exception ex, StatusCode statusCode, string message)
    {
        _logger.LogError(ex, message);
        return new RpcException(new Status(statusCode, message));
    }
}