import { ReactNode } from "react"
import { UserCurrentShift } from "../models/response"

// Trạng thái hành động
export type AttendanceActionType = "IN" | "OUT"

// Cấu hình hiển thị UI
export interface AttendanceUIConfig {
    color: "primary" | "secondary" | "danger" | "warning" | "default"
    hexColor: string // Tailwind class như 'bg-danger'
    icon: ReactNode
    label: string
    description: string
}

// Kết quả trả về từ useAttendanceWatcher
export interface AttendanceStatus {
    shift: UserCurrentShift
    isComingUp: boolean
    isLate: boolean
    isOngoing: boolean
    isOvertime: boolean
    shouldWarn: boolean
    canCheckOut: boolean // Flag từ Backend + logic window
    deadlineMinutes: number
}
