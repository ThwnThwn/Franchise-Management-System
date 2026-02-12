/**
 * ATTENDANCE CONSTANTS
 * Đồng bộ trực tiếp từ FranchiseService.Domain.Constants
 */
export const ATTENDANCE_CONFIG = {
    // Thời gian cho phép Check-in trước khi ca bắt đầu (Phút)
    CHECK_IN_BUFFER_MINUTES: 30,

    // Thời gian vẫn cho phép Check-out sau khi ca kết thúc (Giờ)
    CHECK_OUT_BUFFER_HOURS: 2,

    // Khoảng cách tối đa để Check-in/out (Mét)
    MAX_GPS_DISTANCE_METERS: 100.0
} as const
