using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using IdentityService.Application.AppExceptions;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.API.Interceptors;

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
        catch (ValidationException ex) // FluentValidation
        {
            var msg = string.Join(" | ", ex.Errors.Select(x => x.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, msg));
        }
        catch (ForbidenException ex)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
        }
        catch (ConflictDuplicateException ex)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
        catch (Application.AppExceptions.DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error");
            throw new RpcException(new Status(StatusCode.Aborted, "Database update error"));
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }
}