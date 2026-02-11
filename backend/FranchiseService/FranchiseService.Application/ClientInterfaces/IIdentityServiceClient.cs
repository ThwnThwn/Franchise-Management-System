namespace FranchiseService.Application.ClientInterfaces
{
    /// <summary>
    /// Interface giao tiếp với IdentityService qua gRPC.
    /// Dùng để validate user tồn tại và lấy thông tin nhân viên.
    /// </summary>
    public interface IIdentityServiceClient
    {
        /// <summary>
        /// Lấy thông tin user theo ID từ IdentityService.
        /// Trả về null nếu không tìm thấy.
        /// </summary>
        Task<UserInfo?> GetUserByIdAsync(Guid userId);
    }

    /// <summary>
    /// DTO chứa thông tin user từ IdentityService.
    /// Chỉ lấy các field cần dùng trong FranchiseService.
    /// </summary>
    public class UserInfo
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal? CurrentHourlyWage { get; set; }
    }
}
