using Grpc.Core;

namespace FranchiseService.API.Extentions;

public static class GrpcMetadataExtensions
{
    /// <summary>
    /// Lấy User ID từ Header x-user-id trong gRPC Metadata
    /// </summary>
    public static Guid? GetUserId(this ServerCallContext context)
    {
        // gRPC Metadata keys luôn là lowercase (tiêu chuẩn HTTP/2)
        var headerValue = context.RequestHeaders.GetValue("x-user-id");

        if (string.IsNullOrEmpty(headerValue))
        {
            return null;
        }

        if (Guid.TryParse(headerValue, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// Helper lấy giá trị string bất kỳ từ header
    /// </summary>
    public static string? GetHeaderValue(this ServerCallContext context, string key)
    {
        return context.RequestHeaders.GetValue(key.ToLowerInvariant());
    }
}