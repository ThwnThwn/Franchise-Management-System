using EngagementService.Application.AppExceptions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Sprache; // Thêm namespace này để check môi trường

namespace EngagementService.API.Interceptors;

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
        // Sử dụng tên đầy đủ để không bị nhầm với bất kỳ class nào khác
        catch (FluentValidation.ValidationException ex)
        {
            var metadata = new Metadata();
            foreach (var error in ex.Errors)
            {
                // KHÔNG đưa Message tiếng Việt vào Key hoặc Value của Metadata nếu không dùng hậu tố -bin
                // Cách tốt nhất: Chỉ đưa PropertyName vào Metadata để Client biết lỗi ở đâu
                var key = $"error-{error.PropertyName.ToLower().Replace(".", "-")}";
                metadata.Add(key, "validation_failed"); // Dùng code tiếng Anh đơn giản ở đây
            }

            // Tiếng Việt có dấu chỉ nên để ở phần Status Message (gRPC hỗ trợ UTF-8 ở đây)
            var detail = "Dữ liệu không hợp lệ: " + string.Join(", ", ex.Errors.Select(x => x.ErrorMessage));

            throw new RpcException(new Status(StatusCode.InvalidArgument, detail), metadata);
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
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (DbUpdateException ex)
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