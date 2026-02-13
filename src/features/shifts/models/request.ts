import { DayOfWeek } from "@/constants/enums"
import { ShiftStatus } from "../constants/enum"

// ===============================
// Shift
// ===============================
export interface GetShiftsParams {
    storeId: string
    fromDate?: string
    toDate?: string
    status?: ShiftStatus
}

export interface CreateShiftRequest {
    storeId: string
    shiftDate: string
    startTime: string
    endTime: string
    requiredStaffCount: number
    breakDurationMinutes: number
    note?: string
    shiftTemplateId?: string
}

export interface UpdateShiftRequest {
    shiftDate?: string
    startTime?: string
    endTime?: string
    requiredStaffCount?: number
    breakDurationMinutes?: number
    note?: string
    status?: ShiftStatus
    shiftTemplateId?: string
}

// ===============================
// Shift bulk
// ===============================
/**
 * DTO cho việc tạo ca làm việc thủ công hàng loạt.
 * Hệ thống sẽ quét từ startDate đến endDate, ngày nào khớp với daysOfWeek sẽ tạo 1 ca Draft.
 */
export interface BulkCreateManualRequest {
    storeId: string
    startTime: string // HH:mm (VD: "08:00")
    endTime: string // HH:mm (VD: "12:00")
    daysOfWeek: DayOfWeek[] // Mảng các thứ cần tạo [1, 3, 5]
    startDate: string // YYYY-MM-DD
    endDate: string // YYYY-MM-DD

    requiredStaffCount: number // Số nhân viên cần cho mỗi ca
    breakDurationMinutes: number // Thời gian nghỉ
    note?: string // Ghi chú chung
}

/**
 * DTO cho việc đổ lịch từ Template hàng loạt.
 */
export interface BulkCreateFromTemplateRequest {
    storeId: string
    templateId: string // ID của mẫu ca làm việc (Sáng/Chiều/Tối)
    startDate: string // YYYY-MM-DD
    endDate: string // YYYY-MM-DD
    requiredStaffCount: number
}

// ===============================
// Shift template
// ===============================
export interface GetShiftTemplatesParams {
    storeId?: string
    isActive?: boolean
    keyword?: string
}

export interface CreateShiftTemplateRequest {
    storeId?: string
    code: string
    name: string
    startTime: string
    endTime: string
    breakDurationMinutes: number
}

export interface UpdateShiftTemplateRequest {
    code?: string
    name?: string
    startTime?: string
    endTime?: string
    breakDurationMinutes?: number
    isActive?: boolean
}

// ===============================
// Shift rule
// ===============================
export interface SaveShiftRuleRequest {
    storeId?: string // Truyền ID store để tạo/cập nhật store rule
    allowShuttle: boolean
    allowShiftSwap: boolean
    minHoursBetweenShifts: number
    maxHoursPerWeek: number
}

// ===============================
// Shift availability
// ===============================

/**
 * Tham số dùng để filter danh sách lịch rảnh/bận
 */
export interface GetAvailabilityParams {
    // Filter theo thời gian
    fromDate?: string // YYYY-MM-DD
    toDate?: string // YYYY-MM-DD

    // Filter theo trạng thái và loại lịch
    isAvailable?: boolean
    isRecurring?: boolean
    dayOfWeek?: DayOfWeek

    // Dành cho Manager filter theo Store hoặc Nhân viên cụ thể
    storeId?: string
    userId?: string

    // Tìm kiếm nhanh theo Note
    keyword?: string
}

export interface CreateShiftAvailabilityRequest {
    isAvailable: boolean
    isRecurring: boolean
    specificDate?: string
    dayOfWeek?: DayOfWeek
    startTime: string
    endTime: string
    note?: string
}

export interface UpdateShiftAvailabilityRequest extends Partial<CreateShiftAvailabilityRequest> {}

// ===============================
// My schedule (Staff)
// ===============================
export interface AttendancePayload {
    latitude?: number
    longitude?: number
    note?: string
}

export interface GetAgendaParams {
    startDate: string // "YYYY-MM-DD"
    endDate: string // "YYYY-MM-DD"
}
