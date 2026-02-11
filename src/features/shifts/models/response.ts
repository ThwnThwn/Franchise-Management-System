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
export interface StaffAssignmentCandidate {
    id: string
    firstName: string
    lastName?: string
    avatarUrl?: string
    currentHourlyWage: number

    isAvailable: boolean
    note?: string

    // Dữ liệu check chéo với bảng shift_assignments
    isConflict: boolean // check xem có record nào trùng giờ chưa
    isAssigned: boolean // check xem staff đã có trong ca này chưa
}
