using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistences.Caches;

public interface IOtpStore
{
    Task<string?> GetOtpAsync(string key, string purpose);

    Task SaveOTPAsyns(string key, string otp, string purpose);

    Task RemoveOTPAsync(string key, string purpose);

    Task SaveRegistrationDataAsync(string key ,string data);

    Task SaveForgetPasswordDataAsync(string key , string data);

    Task<string?> GetRegistrationDataAsync(string key);

    Task RemoveRegistrationDataAsync(string key);

    Task RemoveForgetPasswordDataAsync(string key);

    Task<string?> GetForgetPasswordAsync(string key);

    /// <summary>
    /// Lấy thời điểm request OTP lần cuối cùng để thực hiện rate limiting.
    /// Dùng để ngăn chặn spam OTP bằng cách áp dụng thời gian chờ giữa các lần request.
    /// </summary>
    /// <param name="key">Định danh duy nhất (thường là email) liên kết với request OTP.</param>
    /// <returns>
    /// Thời điểm <see cref="DateTimeOffset"/> khi OTP được request lần cuối,
    /// hoặc null nếu chưa có request trước đó hoặc đã hết thời gian chờ.
    /// </returns>
    Task<DateTimeOffset?> GetLastOtpRequestTimeAsync(string key, string purpose);

    /// <summary>
    /// Lưu thời điểm khi OTP được request để thực thi rate limiting.
    /// Thời điểm này được dùng để tính toán thời gian chờ trước khi cho phép request OTP tiếp theo.
    /// Tự động hết hạn sau 5 phút trong Redis.
    /// </summary>
    /// <param name="key">Định danh duy nhất (thường là email) liên kết với request OTP.</param>
    /// <param name="time">Thời điểm khi OTP được request.</param>
    Task SaveLastOtpRequestTimeAsync(string key, DateTimeOffset time, string purpose);

    /// <summary>
    /// Lấy số lần thử xác thực OTP sai hiện tại.
    /// Dùng để ngăn chặn tấn công brute force bằng cách giới hạn số lần nhập OTP sai.
    /// </summary>
    /// <param name="key">Định danh duy nhất (thường là email) liên kết với việc xác thực OTP.</param>
    /// <returns>
    /// Số lần thử xác thực OTP thất bại. Trả về 0 nếu chưa có lần thử nào
    /// hoặc bộ đếm đã được reset.
    /// </returns>
    Task<int> GetOtpAttemptCountAsync(string key, string purpose);

    /// <summary>
    /// Tăng bộ đếm số lần thử xác thực OTP sai lên 1.
    /// Được gọi mỗi khi OTP nhập vào không đúng. Khi bộ đếm đạt số lần tối đa
    /// cho phép (thường là 5), OTP sẽ bị vô hiệu hóa và phải request lại.
    /// </summary>
    /// <param name="key">Định danh duy nhất (thường là email) liên kết với việc xác thực OTP.</param>
    Task IncrementOtpAttemptAsync(string key, string purpose);

    /// <summary>
    /// Reset bộ đếm số lần thử xác thực OTP sai về 0.
    /// Được gọi khi OTP mới được tạo hoặc khi xác thực OTP thành công,
    /// cho phép người dùng bắt đầu lại với số lần thử mới.
    /// </summary>
    /// <param name="key">Định danh duy nhất (thường là email) liên kết với việc xác thực OTP.</param>
    Task ResetOtpAttemptAsync(string key, string purpose);
}