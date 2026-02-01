using SharedKernel.Events; // Or specific DTO if needed

namespace NotificationService.Application.ExternalServices;

/// <summary>
/// Interface giao tiếp với IdentityService qua gRPC.
/// Dùng để lấy thông tin người dùng (Email, Tên...) phục vụ cho việc gửi thông báo.
/// </summary>
public interface IIdentityServiceClient
{
    /// <summary>
    /// Lấy Email của người dùng dựa trên UserId.
    /// Hàm này gọi sang IdentityService qua gRPC để đảm bảo luôn lấy được email mới nhất.
    /// </summary>
    /// <param name="userId">ID của người dùng.</param>
    /// <returns>Email của người dùng hoặc null nếu không tìm thấy.</returns>
    Task<string> GetUserEmailByIdAsync(string userId);
}
