import { StaffAssignmentCandidate } from "@/features/shifts/models/response"

export const getValidationErrors = (item: StaffAssignmentCandidate) => {
    const errors: string[] = []
    const { validation, isConflict } = item

    if (isConflict) errors.push("Trùng lịch với ca làm việc khác")
    if (!item.isAvailable) errors.push("Nhân viên có lịch bận cá nhân")

    if (validation) {
        const { isShuttleInvalid, isRestTimeInvalid, isOverLimit, restHours, weeklyHours } =
            validation

        if (isShuttleInvalid) errors.push("Cửa hàng không hỗ trợ điều chuyển")
        if (isRestTimeInvalid) errors.push(`Nghỉ quá ngắn (${restHours}h)`)
        if (isOverLimit) errors.push(`Vượt định mức tuần (${weeklyHours}h)`)
    }

    return errors
}
