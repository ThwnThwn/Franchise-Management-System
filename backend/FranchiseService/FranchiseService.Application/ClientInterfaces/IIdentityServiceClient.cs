namespace FranchiseService.Application.ClientInterfaces
{
    // Interface giao tiep voi IdentityService qua gRPC
    // Dung de validate user ton tai va lay thong tin nhan vien
    public interface IIdentityServiceClient
    {
        // Lay thong tin user theo ID tu IdentityService
        // Tra ve null neu khong tim thay
        Task<UserInfo?> GetUserByIdAsync(Guid userId);

        // Lay danh sach tat ca user (co the filter theo role)
        // Dung cho GetAvailableStaffs de lay danh sach nhan vien cua store
        Task<List<UserInfo>> GetAllUsersAsync(string? roleSlug = null);

        // Lay nhieu user bang 1 request (tranh N cong 1)
        Task<List<UserInfo>> GetUsersByIdsAsync(IEnumerable<Guid> userIds);
    }

    // DTO chua thong tin user tu IdentityService
    // Chi lay cac field can dung trong FranchiseService
    public class UserInfo
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal? CurrentHourlyWage { get; set; }

        // Cac field bo sung cho StaffAssignmentCandidate
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }

        // ID cua hang "nha" cua nhan vien (homeStoreId trong Identity)
        public Guid? HomeStoreId { get; set; }
    }
}
