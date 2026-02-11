import { DayOfWeek } from "@/constants/enums"
import { ShiftStatus } from "../constants/enum"

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
