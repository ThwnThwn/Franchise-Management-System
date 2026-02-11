import { DayOfWeek } from "@/constants/enums"
import { ShiftAssignmentStatus, ShiftStatus } from "../constants/enum"

export interface ShiftAssignment {
    id: string
    shiftId: string
    userId: string
    status: ShiftAssignmentStatus
    hourlyWage: number

    staffName: string
    staffAvatarUrl?: string
    staffEmail?: string
}

export interface ShiftResponse {
    id: string
    storeId: string
    shiftDate: string
    startTime: string
    endTime: string
    requiredStaffCount: number
    breakDurationMinutes: number

    shiftTemplateId?: string
    status: ShiftStatus
    note?: string

    assignedStaffCount: number

    assignments: ShiftAssignment[]
}

// ===============================
// Shift template
// ===============================
export interface ShiftTemplateResponse {
    id: string
    storeId?: string
    code: string
    name: string
    startTime: string
    endTime: string
    breakDurationMinutes: number
    isActive: boolean
    createdAt: string
}

// ===============================
// Shift assignment
// ===============================
/**
 * DTO đại diện cho một ứng viên nhân viên có thể gán vào ca làm.
 * Dữ liệu validation được tính toán dựa trên context của ca làm (shiftId/thời gian/cửa hàng).
 */
export interface StaffAssignmentCandidate {
    id: string
    firstName: string
    lastName?: string
    avatarUrl?: string
    currentHourlyWage: number

    // --- TRẠNG THÁI KHẢ DỤNG (AVAILABILITY) ---
    // Dựa trên lịch đăng ký rảnh/bận của nhân viên (Staff Availability)
    isAvailable: boolean
    // Ghi chú từ lịch rảnh (VD: "Chỉ làm được ca tối", "Bận việc gia đình")
    note?: string

    // --- TRẠNG THÁI GÁN CA ---
    // true nếu nhân viên đã nằm trong danh sách gán của ca này (tránh add trùng)
    isAssigned: boolean

    /**
     * RULE 1: CONFLICT TIME (TRÙNG LỊCH)
     * Trạng thái chặn cứng (Hard Block).
     * true nếu nhân viên đã có một ca làm khác (ở bất kỳ Store nào)
     * giao thoa về mặt thời gian với ca hiện tại.
     */
    isConflict: boolean

    // --- BỘ DỮ LIỆU KIỂM TRA QUY ĐỊNH (BUSINESS RULES VALIDATION) ---
    // BE tính toán dựa trên Settings của Store (min_rest, max_hours, allow_shuttle)
    validation?: {
        /**
         * RULE 2: REST PERIOD (THỜI GIAN NGHỈ)
         * isRestTimeInvalid: true nếu khoảng nghỉ từ ca trước đến ca này < quy định.
         * restHours: Số giờ nghỉ thực tế giữa 2 ca (Dùng để FE render message).
         */
        isRestTimeInvalid: boolean
        restHours: number

        /**
         * RULE 3: WEEKLY CAP (GIỚI HẠN GIỜ TUẦN)
         * isOverLimit: true nếu tổng giờ trong tuần (bao gồm ca này) > quy định.
         * weeklyHours: Tổng số giờ nhân viên đã gán trong tuần hiện tại (trước ca này).
         */
        isOverLimit: boolean
        weeklyHours: number

        /**
         * RULE 4: SHUTTLE CHECK (ĐIỀU CHUYỂN)
         * true nếu: Ca ở Store A, nhân viên thuộc Store B
         * NHƯNG một trong hai Store không cho phép điều chuyển (allow_shuttle = false).
         */
        isShuttleInvalid: boolean
    }
}

// ===============================
// Shift rule
// ===============================
export interface ShiftRuleResponse {
    id: string
    storeId?: string
    allowShuttle: boolean
    allowShiftSwap: boolean
    minHoursBetweenShifts: number
    maxHoursPerWeek: number
    updatedAt: string
}

// Kiểu trả về mới khi Manager xem cấu hình
export interface ShiftRuleSettingsResponse {
    global: ShiftRuleResponse

    // dùng mảng do nếu admin xem thì lấy hết store lên
    // manager xem thì trong query param có truyền store id và trong mảng chỉ có 0 / 1 item
    stores: ShiftRuleResponse[]
}

// ===============================
// Shift availability
// ===============================
export interface ShiftAvailabilityResponse {
    id: string
    userId: string
    isAvailable: boolean
    isRecurring: boolean
    specificDate?: string
    dayOfWeek?: DayOfWeek
    startTime: string // HH:mm:ss từ BE
    endTime: string
    note?: string
    createdAt: string // Dùng để hiển thị "Ngày gửi" trong Table
    updatedAt: string // Dùng để hiển thị "Cập nhật cuối"
}
