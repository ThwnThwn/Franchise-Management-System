using System.Runtime.InteropServices;

namespace SalesService.Application.Constants;

public static class DateConstants
{
    // Tên múi giờ trên Windows
    public const string WindowsVietnamTimeZoneId = "SE Asia Standard Time";
    
    // Tên múi giờ trên Linux/Docker
    public const string LinuxVietnamTimeZoneId = "Asia/Ho_Chi_Minh";

    // Hàm tiện ích để lấy đúng ID tùy theo môi trường
    public static string GetVietnamTimeZoneId() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
            ? WindowsVietnamTimeZoneId 
            : LinuxVietnamTimeZoneId;
}