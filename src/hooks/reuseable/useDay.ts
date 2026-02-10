import {
    DEFAULT_DATE_TIME_FORMAT,
    DEFAULT_TIMEZONE,
    DEFAULT_DATE_FORMAT
} from "@/constants/constants"
import dayjs from "dayjs"
import utc from "dayjs/plugin/utc"
import timezone from "dayjs/plugin/timezone"
import {
    DateValue,
    parseDate,
    parseZonedDateTime,
    ZonedDateTime,
    parseTime,
    Time,
    CalendarDate
} from "@internationalized/date"

// Kích hoạt plugin để xử lý Timezone chuẩn xác
dayjs.extend(utc)
dayjs.extend(timezone)

export const useDay = ({
    defaultFormat = DEFAULT_DATE_TIME_FORMAT,
    timeZone = DEFAULT_TIMEZONE
}: {
    defaultFormat?: string
    timeZone?: string
} = {}) => {
    /**
     * Chuyển đổi String/Date -> Time Object (Dùng cho TimeInput)
     * Input: "08:00:00" | "08:00"
     */
    const toTime = (timeStr: string | null | undefined): Time | null => {
        if (!timeStr) return null
        try {
            // Clean string: bỏ mili giây hoặc timezone thừa nếu có
            const cleanTime = timeStr.split("T")[1] || timeStr // Handle if full ISO string passed
            const finalTime = cleanTime.split(".")[0].split("+")[0].split("Z")[0].trim()
            return parseTime(finalTime)
        } catch {
            return null
        }
    }

    /**
     * Format Time Object -> String chuẩn (Gửi API)
     * Output: "08:00:00"
     */
    const formatTime = (time: Time | null | undefined): string => {
        if (!time) return ""
        return time.toString()
    }

    /**
     * Format Date -> YYYY-MM-DD (Gửi API)
     */
    const formatDateApi = (
        date: DateValue | string | Date | dayjs.Dayjs | null | undefined
    ): string => {
        if (!date) return ""
        if (typeof date === "object" && "toString" in date) return date.toString()
        return dayjs(date).format(DEFAULT_DATE_FORMAT)
    }

    const toZonedDateTime = (
        dateTime: string | number | Date | dayjs.Dayjs | null | undefined
    ): ZonedDateTime | null => {
        if (!dateTime) return null

        const d = dayjs(dateTime)
        if (!d.isValid()) return null

        // Tạo format chuẩn cho parseZonedDateTime bất kể input
        // Format yêu cầu: YYYY-MM-DDTHH:mm:ss[TimeZone]
        const isoString = d.format("YYYY-MM-DDTHH:mm:ss")
        return parseZonedDateTime(`${isoString}[${timeZone}]`)
    }

    const toDate = (
        date: string | number | Date | dayjs.Dayjs | null | undefined
    ): CalendarDate | null => {
        if (!date) return null

        const d = dayjs(date)
        if (!d.isValid()) return null

        // Cải tiến: Luôn format về YYYY-MM-DD để parseDate không bị lỗi với string có giờ
        return parseDate(d.format(DEFAULT_DATE_FORMAT))
    }

    // Giữ nguyên signature nhận object params như cũ
    const formatDateTime = ({
        date,
        timeZone: tz = timeZone // Dùng alias để ưu tiên tham số truyền vào, fallback về prop của hook
    }: {
        date: DateValue | string | null | undefined // Cho phép null/undefined để an toàn
        timeZone?: string
    }): string => {
        if (!date) return ""

        // Nếu là string, format trực tiếp
        if (typeof date === "string") {
            return dayjs(date).format(defaultFormat)
        }

        // Nếu là DateValue (ZonedDateTime, CalendarDate)
        // Chuyển về native Date rồi format
        const dateJs = dayjs(date.toDate(tz))
        return dateJs.format(defaultFormat)
    }

    const getDiffDaysCeil = ({
        startDate,
        endDate
    }: {
        startDate?: string | Date | dayjs.Dayjs | null
        endDate?: string | Date | dayjs.Dayjs | null
    }) => {
        if (!startDate || !endDate) return -1
        return Math.ceil(dayjs(endDate).diff(dayjs(startDate), "day", true))
    }

    const normalizeDateByMinuteStep = (
        currentValue: DateValue,
        prevValue?: DateValue,
        step = 5
    ) => {
        if (!("minute" in currentValue)) return currentValue

        const curr = currentValue as ZonedDateTime
        const prev = prevValue as ZonedDateTime | undefined

        let isIncreasing = true

        if (prev) {
            if (prev.minute === curr.minute) return curr
            if (prev.minute == 0 && curr.minute == 59 && curr.hour == prev.hour) {
                isIncreasing = false
            } else if (prev.minute >= curr.minute) {
                isIncreasing = false
            }
        }

        let newMinute = isIncreasing
            ? Math.ceil(curr.minute / step) * step
            : Math.floor(curr.minute / step) * step

        let newDate = curr

        if (isIncreasing && newMinute === 60) {
            newDate = newDate.add({ hours: 1 })
            newMinute = 0
        }

        if (!isIncreasing && newMinute === 55) {
            newDate = newDate.add({ hours: -1 })
            newMinute = 60 - step
        }

        const rounded = newDate.set({ minute: newMinute, second: 0 })
        return rounded
    }

    return {
        toTime,
        formatTime,
        formatDateApi,
        toDate,
        toZonedDateTime,
        formatDateTime,
        getDiffDaysCeil,
        normalizeDateByMinuteStep
    }
}
