import { StaffAssignmentCandidate } from "../../models/response"

export const getValidationErrors = (item: StaffAssignmentCandidate) => {
    const errors: string[] = []
    if (item.isConflict) errors.push("Trùng lịch với ca làm việc khác")

    if (item.validation) {
        if (item.validation.isShuttleInvalid)
            errors.push("Cửa hàng không hỗ trợ điều chuyển nhân sự")
        if (item.validation.isRestTimeInvalid)
            errors.push(`Nghỉ quá ngắn giữa 2 ca (chỉ có ${item.validation.restHours}h)`)
        if (item.validation.isOverLimit)
            errors.push(`Vượt định mức giờ tuần (đã làm ${item.validation.weeklyHours}h)`)
    }
    return errors
}
