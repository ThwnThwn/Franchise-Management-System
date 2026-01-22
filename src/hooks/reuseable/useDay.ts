import { DEFAULT_DATE_TIME_FORMAT, DEFAULT_TIMEZONE } from "@/constants/constants"
import dayjs from "dayjs"
import { DateValue, parseDate, parseZonedDateTime, ZonedDateTime } from "@internationalized/date"

export const useDay = ({
    defaultFormat = DEFAULT_DATE_TIME_FORMAT,
    timeZone = DEFAULT_TIMEZONE
}: {
    defaultFormat?: string
    timeZone?: string
} = {}) => {
    const toZonedDateTime = (dateTime: string | number | Date | dayjs.Dayjs | null | undefined) => {
        if (!dateTime) return null

        if (typeof dateTime === "string") {
            dateTime = dayjs(dateTime).format(defaultFormat)
            dateTime = dateTime.split("+")[0]
        }

        const d = dayjs(dateTime)

        const str = `${d.format(defaultFormat).split("+")[0]}[${timeZone}]`
        return parseZonedDateTime(str)
    }

    const toDate = (date: string | number | Date | dayjs.Dayjs | null | undefined) => {
        if (!date) return null
        const str = dayjs(date).format(defaultFormat)
        return parseDate(str)
    }

    const formatDateTime = ({
        date,
        timeZone = DEFAULT_TIMEZONE
    }: {
        date: DateValue | string
        timeZone?: string
    }): string => {
        if (!date) return ""
        if (typeof date === "string") {
            return dayjs(date).format(defaultFormat)
        }

        const dateJs = dayjs(date.toDate(timeZone))
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

    return { toDate, toZonedDateTime, formatDateTime, getDiffDaysCeil, normalizeDateByMinuteStep }
}
