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
