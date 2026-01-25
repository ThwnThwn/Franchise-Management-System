namespace IdentityService.Application.Persistences.Caches;

public interface ITokenBlacklist
{
    /// <summary>
    /// Thêm access token vào blacklist
    /// </summary>
    /// <param name="accessToken">Access token cần blacklist</param>
    /// <param name="expiryTime">Thời gian token hết hạn (để tự động xóa khỏi Redis)</param>
    Task AddToBlacklistAsync(string accessToken, DateTimeOffset expiryTime);

    /// <summary>
    /// Kiểm tra token có trong blacklist không
    /// </summary>
    /// <param name="accessToken">Access token cần kiểm tra</param>
    /// <returns>True nếu token trong blacklist, False nếu không</returns>
    Task<bool> IsTokenBlacklistedAsync(string accessToken);

    /// <summary>
    /// Xóa token khỏi blacklist (optional, thường không cần vì Redis tự expire)
    /// </summary>
    Task RemoveFromBlacklistAsync(string accessToken);
}
