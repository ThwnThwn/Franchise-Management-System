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
