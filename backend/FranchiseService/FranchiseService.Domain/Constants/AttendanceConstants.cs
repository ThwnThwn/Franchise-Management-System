namespace FranchiseService.Domain.Constants
{
    public static class AttendanceConstants
    {
        // Thời gian cho phép Check-in trước khi ca bắt đầu (Phút)
        public const int CheckInBufferMinutes = 30;

        // Thời gian vẫn cho phép Check-out sau khi ca kết thúc (Giờ)
        // Dùng số dương ở đây, lúc tính toán sẽ dùng .AddHours(-Value)
        public const int CheckOutBufferHours = 2;

        // Bonus: Khoảng cách tối thiểu để Check-in (nếu dùng GPS)
        public const double MaxGpsDistanceMeters = 100.0;
    }
}