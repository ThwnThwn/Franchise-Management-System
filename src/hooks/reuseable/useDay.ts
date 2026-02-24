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
        time: Time | CalendarDateTime | ZonedDateTime | Date | string | null | undefined,
        withSecond: boolean = true
    ): string => {
        // Handle Null/Undefined/Empty
        if (!time) return ""

        let hour: string = "00"
        let minute: string = "00"
        let second: string = "00"

        // Nếu là String (HH:mm:ss hoặc HH:mm)
        if (typeof time === "string") {
            const parts = time.split(":")
            hour = parts[0]?.padStart(2, "0") || "00"
            minute = parts[1]?.padStart(2, "0") || "00"
            second = parts[2]?.padStart(2, "0") || "00"
        }
        // Nếu là Native JS Date
        else if (time instanceof Date) {
            hour = time.getHours().toString().padStart(2, "0")
            minute = time.getMinutes().toString().padStart(2, "0")
            second = time.getSeconds().toString().padStart(2, "0")
        }
        // Nếu là Object (@internationalized/date)
        else if ("hour" in time) {
            hour = time.hour.toString().padStart(2, "0")
            minute = time.minute.toString().padStart(2, "0")
            // @ts-ignore - lấy second nếu có
            second = (time.second ?? 0).toString().padStart(2, "0")
        } else {
            return ""
        }

        // Build kết quả dựa trên withSecond
        return withSecond ? `${hour}:${minute}:${second}` : `${hour}:${minute}`
    }

    /**
     * Format Date -> String "YYYY-MM-DD" (Gửi API - chỉ lấy ngày)
     */
    const formatDate = (
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
    /**
     * Format DateTime -> String hiển thị đẹp (Display)
     * Chấp nhận: string, native Date, hoặc HeroUI DateValue
     */
    /**
     * Hàm check xem có phải là DateValue của HeroUI/NextUI không
     */
    const isHeroUIDate = (date: any): date is DateValue => {
        return date && typeof date.toDate === "function"
    }

    const formatDateTime = ({
        date,
        timeZone: tz = "Asia/Ho_Chi_Minh",
        format = defaultFormat
    }: {
        date: DateValue | dayjs.Dayjs | string | Date | null | undefined
        timeZone?: string
        format?: string
    }): string => {
        if (!date) return ""

        let result: dayjs.Dayjs

        // 1. Nếu là Dayjs
        if (dayjs.isDayjs(date)) {
            result = date
        }
        // 2. Nếu là DateValue (Sử dụng Type Guard để TS tự hiểu)
        else if (isHeroUIDate(date)) {
            result = dayjs(date.toDate(tz))
        }
        // 3. Các trường hợp còn lại: string | Date
        else {
            // Lúc này TS đã biết date KHÔNG THỂ là DateValue hay Dayjs nữa
            // nên nó sẽ cho phép truyền vào dayjs() mà không cần ép kiểu
            result = dayjs(date)
        }

        return result.isValid() ? result.tz(tz).format(format) : ""
    }

    // ========================================================================
    // 3. HELPERS (Các hàm tiện ích tính toán)
    // ========================================================================
    /**
     * Lấy ngày hiện tại
     * @param dayBuffer số hoặc undefined
     * @returns ngày hiện tiện theo format DEFAULT_DATE_FORMAT (YYYY-MM-DD)
     */
    const dateNow = (dayBuffer?: number) => {
        return dayjs()
            .add(dayBuffer ?? 0, "day")
            .format(DEFAULT_DATE_FORMAT)
    }

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
        formatDate,
        formatToApiUTC,
        formatDateTime,

        // Helpers
        dateNow,
        getDiffDaysCeil,
        normalizeDateByMinuteStep
    }
}
