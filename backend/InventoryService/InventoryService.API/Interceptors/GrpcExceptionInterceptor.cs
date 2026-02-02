using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using InventoryService.Application.AppExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Sprache; // Thêm namespace này để check môi trường

namespace InventoryService.API.Interceptors;

public class GrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionInterceptor> _logger;
    private readonly IHostEnvironment _env;

    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
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
        catch (ValidationException ex)
        {
            // Trả về Metadata để Postman hiển thị ở tab Trailers/Metadata
            var metadata = new Metadata();
            foreach (var error in ex.Errors)
            {
                metadata.Add($"error-{error.PropertyName.ToLower()}", error.ErrorMessage);
            }

            var msg = "Validation failed: " + string.Join(", ", ex.Errors.Select(x => x.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, msg), metadata);
        }
        catch (BusinessException ex)
        {
            // Business Exception sẽ ra mã số 9 (FailedPrecondition)
            _logger.LogWarning(ex, "Business error occurred");
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Application.AppExceptions.DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error");
            // Trả về thông báo chi tiết hơn nếu là dev
            var message = _env.IsDevelopment() ? ex.InnerException?.Message ?? ex.Message : "Database operation failed";
            throw new RpcException(new Status(StatusCode.Aborted, message));
        }
        catch (OperationCanceledException)
        {
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, "Request was cancelled or timed out"));
        }
        catch (ParseException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Parsing error: {ex.Message}"));
        }
        catch (RpcException)
        {
            throw; // Giữ nguyên các lỗi gRPC đã được ném chủ động
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            // ĐÂY LÀ CHỖ QUAN TRỌNG NHẤT:
            // Nếu là Dev, trả về full lỗi bao gồm cả StackTrace để debug trên Postman
            // Nếu là Prod, mới trả về "Internal Server Error" để bảo mật.
            var detail = _env.IsDevelopment()
                ? $"Internal Error: {ex.Message}\n{ex.StackTrace}"
                : "A critical system error occurred.";

            throw new RpcException(new Status(StatusCode.Internal, detail));
        }
    }
}