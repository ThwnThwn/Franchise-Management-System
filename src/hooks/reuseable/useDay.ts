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
    CalendarDate,
    CalendarDateTime
} from "@internationalized/date"

// Kích hoạt plugin UTC và Timezone cho dayjs
dayjs.extend(utc)
dayjs.extend(timezone)

export const useDay = ({
    defaultFormat = DEFAULT_DATE_TIME_FORMAT,
    timeZone = DEFAULT_TIMEZONE
}: {
    defaultFormat?: string
    timeZone?: string
} = {}) => {
    // ========================================================================
    // 1. PARSERS (Chuyển đổi String/Date -> Object UI Components)
    // ========================================================================

    /**
     * Chuyển đổi String giờ -> Time Object (Dùng cho HeroUI Input Time)
     * Input: "08:00:00" | "08:00" | ISO String
     * Output: Time Object
     */
    const toTime = (timeStr: string | null | undefined): Time | null => {
        if (!timeStr) return null
        try {
            const d = dayjs(timeStr)
            if (!d.isValid()) {
                // Nếu không phải ISO chuẩn, thử parseTime kiểu cũ
                return parseTime(timeStr)
            }
            // Trả về object Time từ giờ và phút của dayjs
            return new Time(d.hour(), d.minute())
        } catch {
            return null
        }
    }

    /**
     * Chuyển đổi String/Date -> CalendarDate (Dùng cho HeroUI DatePicker)
     * Input: "2026-02-10" | Date Object | Dayjs Object
     * Output: CalendarDate (YYYY-MM-DD)
     */
    const toDate = (
        date: string | number | Date | dayjs.Dayjs | null | undefined
    ): CalendarDate | null => {
        if (!date) return null

        const d = dayjs(date)
        if (!d.isValid()) return null

        // Format về YYYY-MM-DD trước khi parse để tránh lỗi timezone
        return parseDate(d.format(DEFAULT_DATE_FORMAT))
    }

    /**
     * Chuyển đổi String/Date -> ZonedDateTime (Dùng cho HeroUI DateTimePicker có Timezone)
     * Input: DateTime String | Date Object
     * Output: ZonedDateTime Object (kèm Timezone default)
     */
    const toZonedDateTime = (
        dateTime: string | number | Date | dayjs.Dayjs | null | undefined
    ): ZonedDateTime | null => {
        if (!dateTime) return null

        const d = dayjs(dateTime)
        if (!d.isValid()) return null

        // Format chuẩn YYYY-MM-DDTHH:mm:ss để ghép timezone
        const isoString = d.format("YYYY-MM-DDTHH:mm:ss")
        return parseZonedDateTime(`${isoString}[${timeZone}]`)
    }

    // ========================================================================
    // 2. FORMATTERS (Chuyển đổi Object -> String chuẩn API/Display)
    // ========================================================================

    /**
     * Format Time Object -> String "HH:mm:ss" (Gửi API)
     */
    const formatTime = (
        time: Time | CalendarDateTime | ZonedDateTime | null | undefined,
        withSecond: boolean = true
    ): string => {
        if (!time || !("hour" in time)) return ""

        // 2. Format giờ và phút
        const hour = time.hour.toString().padStart(2, "0")
        const minute = time.minute.toString().padStart(2, "0")
        let result = `${hour}:${minute}`

        // 3. Nếu cần giây, lấy giá trị second thực tế (ép kiểu để lấy thuộc tính)
        if (withSecond) {
            // Trong @internationalized/date, second thường mặc định là 0 nếu không có
            const second = time.second?.toString().padStart(2, "0") || "00"
            result += `:${second}`
        }

        return result
    }

    /**
     * Format Date -> String "YYYY-MM-DD" (Gửi API - chỉ lấy ngày)
     */
    const formatDateApi = (
        date: DateValue | string | Date | dayjs.Dayjs | null | undefined
    ): string => {
        if (!date) return ""

        // FIX: Chỉ dùng toString() nếu nó là object của HeroUI (có property 'calendar')
        // Còn nếu là JS Date thuần (instanceof Date) thì bỏ qua để xuống dưới dùng dayjs
        if (
            typeof date === "object" &&
            "toString" in date &&
            !(date instanceof Date) && // <--- Thêm điều kiện này
            !dayjs.isDayjs(date) // <--- Và không phải Dayjs object
        ) {
            return date.toString()
        }

        // Các trường hợp còn lại (String, Date, Dayjs) để dayjs lo
        return dayjs(date).format(DEFAULT_DATE_FORMAT)
    }

    /**
     * Format Date -> String UTC ISO [Z] (Gửi API DateTime)
     * Chuyển đổi giờ Local (User) -> Giờ UTC (Server)
     * Input: Local Time (VD: 08:00 VN)
     * Output: UTC Time (VD: 01:00 Z)
     */
    const formatToApiUTC = (
        date: dayjs.Dayjs | Date | string | null | undefined
    ): string | undefined => {
        if (!date) return undefined
        // .utc() tự động trừ giờ theo timezone hiện tại
        return dayjs(date).utc().format(DEFAULT_DATE_TIME_FORMAT)
    }

    /**
     * Format DateTime -> String hiển thị đẹp (Display)
     * Dùng để hiển thị lên UI cho người dùng đọc
     */
    const formatDateTime = ({
        date,
        timeZone: tz = timeZone
    }: {
        date: DateValue | string | null | undefined
        timeZone?: string
    }): string => {
        if (!date) return ""

        // Nếu là string, format trực tiếp
        if (typeof date === "string") {
            return dayjs(date).format(defaultFormat)
        }

        // Nếu là Object của HeroUI, convert sang Date rồi format
        const dateJs = dayjs(date.toDate(tz))
        return dateJs.format(defaultFormat)
    }

    // ========================================================================
    // 3. HELPERS (Các hàm tiện ích tính toán)
    // ========================================================================

    /**
     * Tính số ngày chênh lệch giữa 2 mốc thời gian (làm tròn lên)
     */
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

    /**
     * Làm tròn phút theo bước nhảy (VD: step = 5 -> 00, 05, 10...)
     * Dùng cho việc scroll chọn giờ để trải nghiệm mượt hơn
     */
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

        return newDate.set({ minute: newMinute, second: 0 })
    }

    return {
        // Parsers
        toTime,
        toDate,
        toZonedDateTime,

        // Formatters
        formatTime,
        formatDateApi,
        formatToApiUTC,
        formatDateTime,

        // Helpers
        getDiffDaysCeil,
        normalizeDateByMinuteStep
    }
}
