// features/shifts/utils/shift-mapper.ts
import dayjs from "dayjs"
import { ShiftResponse, StaffShiftAgenda } from "@/features/shifts/models/response"
import { EventInput } from "@fullcalendar/core/index.js"

export const isTimeOnly = (val: string) =>
    val && val.length <= 8 && !val.includes("-") && !val.includes("T")

/**
 * Hợp nhất ShiftDate và Time thành ISO string chuẩn cho FullCalendar
 */
export const getShiftDateTime = (date: string, time: string, referenceStartTime?: string) => {
    if (!isTimeOnly(time)) return time

    let finalDateTime = `${date}T${time}`

    // Xử lý ca đêm: Nếu có referenceStartTime (giờ bắt đầu)
    // và giờ kết thúc < giờ bắt đầu thì cộng thêm 1 ngày
    if (referenceStartTime && isTimeOnly(referenceStartTime) && time < referenceStartTime) {
        const nextDay = dayjs(date).add(1, "day").format("YYYY-MM-DD")
        finalDateTime = `${nextDay}T${time}`
    }

    return finalDateTime
}

/**
 * Chuyển đổi mảng Shift từ API sang mảng Event của FullCalendar
 */
export const mapShiftsToEvents = (shifts: ShiftResponse[]) => {
    return shifts.map((shift) => ({
        id: shift.id,
        title: shift.note || "Ca làm việc",
        start: getShiftDateTime(shift.shiftDate, shift.startTime),
        end: getShiftDateTime(shift.shiftDate, shift.endTime, shift.startTime), // Truyền startTime vào để check ca đêm
        extendedProps: shift,
        backgroundColor: "transparent",
        borderColor: "transparent"
    }))
}

export const mapStaffAgendaToEvents = (agenda: StaffShiftAgenda[]): EventInput[] => {
    return agenda.map((item) => {
        return {
            id: item.id, // assignmentId
            start: getShiftDateTime(item.date, item.startTime),
            end: getShiftDateTime(item.date, item.endTime, item.startTime),
            // Truyền toàn bộ dữ liệu vào extendedProps để hiển thị ở EventCard
            extendedProps: item,
            // Màu sắc cố định cho Staff: Xanh dịu mắt
            backgroundColor: "#3b82f6",
            borderColor: "transparent"
        }
    })
}
