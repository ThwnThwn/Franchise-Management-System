"use client"

import React, { useEffect, useMemo, useState } from "react"
import { Card, CardBody, CardHeader, Divider, useDisclosure } from "@heroui/react"
import { Store, CalendarDays, Info } from "lucide-react"

import { useGetCurrentShift, useAttendanceActions } from "@/features/shifts/hooks"
import { useDay, useDistanceToTarget, useGeolocation } from "@/hooks"
import { ShiftSkeleton } from "./ShiftSkeleton"
import { AttendanceStatus } from "./AttendanceStatus"
import { ShiftCountDown } from "./ShiftCountDown"
import { AttendanceActionButton } from "./AttendanceActionButton"
import { ExceptionNoteModal } from "@/features/shifts/components"
import dayjs from "dayjs"
import { getShiftDateTime } from "@/features/shifts/utils"
import { ATTENDANCE_CONFIG } from "@/features/shifts/constants/config"

export const CurrentShiftCard = () => {
    const { isOpen, onOpen, onClose } = useDisclosure()
    const { formatDate } = useDay()

    // 1. Data Fetching
    const { data: shift, isLoading: isFetchingShift } = useGetCurrentShift()

    const { now, startTime, endTime } = useMemo(() => {
        const now = dayjs()
        // 1. Check shift undefined ngay từ đầu
        if (!shift)
            return {
                now,
                startTime: undefined,
                endTime: undefined
            }

        const todayStr = formatDate(now)

        // 2. Tính toán startTime
        const startTime = dayjs(getShiftDateTime(todayStr, shift.startTime))

        // 3. Tính toán endTime (có truyền startTime để xử lý ca xuyên đêm - cross-day)
        const endTime = dayjs(getShiftDateTime(todayStr, shift.endTime, shift.startTime))

        return {
            now,
            startTime,
            endTime
        }
        // Re-memoize khi object shift thay đổi
    }, [formatDate, shift])

    const { isWithinBuffer, message } = useMemo(() => {
        if (!shift || !startTime || !endTime) return { isWithinBuffer: false, message: "" }

        // 1. Trường hợp chờ Check-in (Status "IN")
        if (!shift.checkInAt) {
            const bufferStart = startTime.subtract(
                ATTENDANCE_CONFIG.CHECK_IN_BUFFER_MINUTES,
                "minute"
            )

            if (now.isBefore(bufferStart)) {
                return {
                    isWithinBuffer: false,
                    message: `Hệ thống sẽ mở Check-in lúc ${bufferStart.format("HH:mm")}`
                }
            }
            if (now.isAfter(endTime)) {
                return {
                    isWithinBuffer: false,
                    message: "Ca làm việc đã kết thúc, bạn đã bỏ lỡ Check-in."
                }
            }
            return { isWithinBuffer: true, message: "" }
        }

        // 2. Trường hợp chờ Check-out (Status "OUT")
        if (shift.checkInAt && !shift.checkOutAt) {
            const bufferEnd = endTime.add(ATTENDANCE_CONFIG.CHECK_OUT_BUFFER_HOURS, "hour")

            if (now.isAfter(bufferEnd)) {
                return {
                    isWithinBuffer: false,
                    message: "Đã quá thời gian cho phép Check-out (2 tiếng sau ca)."
                }
            }
            return { isWithinBuffer: true, message: "" }
        }

        return { isWithinBuffer: false, message: "Bạn đã hoàn thành ca làm việc hôm nay." }
    }, [endTime, now, shift, startTime])

    // 2. Location Logic
    const { getPosition, isLoading: isLocating } = useGeolocation()
    const [currentCoords, setCurrentCoords] = useState<{ lat: number; lng: number } | null>(null)

    const { distanceM, isWithinRange } = useDistanceToTarget(
        currentCoords,
        shift?.storeLat && shift?.storeLong ? { lat: shift.storeLat, lng: shift.storeLong } : null
    )

    // 3. Attendance Actions
    const { handleAction, isLoading: isSubmitting } = useAttendanceActions()

    // 4. Xác định trạng thái nút bấm
    const attendanceStatus = React.useMemo(() => {
        if (!shift?.checkInAt) return "IN"
        if (shift.checkInAt && !shift.checkOutAt) return "OUT"
        return "DONE"
    }, [shift])

    // 5. Xử lý khi bấm nút chính
    const onMainPress = async () => {
        // Luôn làm mới vị trí trước khi bấm
        const pos = await getPosition()

        if (pos.lat && pos.lng) setCurrentCoords({ lat: pos.lat, lng: pos.lng })

        // Nếu máy lỗi GPS hoặc ở quá xa -> Hiện Modal giải trình
        if (pos.error || (distanceM && !isWithinRange)) {
            onOpen()
            return
        }

        // Nếu mọi thứ OK -> Chấm công thẳng luôn
        await handleAction({
            type: attendanceStatus as "IN" | "OUT",
            id: (attendanceStatus === "IN" ? shift?.assignmentId : shift?.currentLogId) || "",
            target: { lat: shift?.storeLat || 0, lng: shift?.storeLong || 0 }
        })
    }

    // 6. Xử lý gửi giải trình từ Modal
    const onExceptionSubmit = async (userNote: string) => {
        await handleAction({
            type: attendanceStatus as "IN" | "OUT",
            id: (attendanceStatus === "IN" ? shift?.assignmentId : shift?.currentLogId) || "",
            target: { lat: shift?.storeLat || 0, lng: shift?.storeLong || 0 },
            userNote
        })
        onClose()
    }

    useEffect(() => {
        const initLocation = async () => {
            // Chỉ lấy vị trí nếu hôm nay có ca làm để tránh tốn pin/resource vô ích
            if (shift?.hasShiftToday) {
                const pos = await getPosition()
                if (pos.lat && pos.lng) {
                    setCurrentCoords({ lat: pos.lat, lng: pos.lng })
                }
            }
        }

        initLocation()
    }, [shift?.hasShiftToday, getPosition])

    if (isFetchingShift || !startTime || !endTime) return <ShiftSkeleton />

    if (!shift?.hasShiftToday)
        return (
            <Card className="bg-default-50 border-none p-6 text-center text-default-400">
                Hôm nay bạn không có ca làm việc nào hoặc chưa đến giờ check-in.
            </Card>
        )

    return (
        <>
            <Card className="shadow-sm border border-default-200">
                <CardHeader className="flex justify-between items-start p-5">
                    <div className="flex gap-3">
                        <div className="p-3 bg-secondary rounded-2xl text-white">
                            <Store size={24} />
                        </div>
                        <div className="flex flex-col">
                            <span className="text-lg font-bold">{shift.storeName}</span>
                            <AttendanceStatus
                                distanceM={distanceM}
                                isWithinRange={isWithinRange}
                                isLoading={isLocating}
                            />
                        </div>
                    </div>
                    <div className="flex flex-col items-end">
                        <div className="flex items-center gap-1 text-default-500 text-tiny">
                            <CalendarDays size={14} />
                            Hôm nay
                        </div>
                        <span className="font-bold text-default-700">
                            {shift.startTime} - {shift.endTime}
                        </span>
                    </div>
                </CardHeader>

                <Divider />

                <CardBody className="p-5 flex flex-col gap-3">
                    {isWithinBuffer ? (
                        <>
                            {/* HIỆN: Countdown & Nút bấm khi nằm trong buffer */}
                            <ShiftCountDown
                                targetTime={!shift.checkInAt ? startTime : endTime}
                                label={
                                    !shift.checkInAt
                                        ? "Thời gian cho tới khi ca bắt đầu"
                                        : "Thời gian kết thúc ca"
                                }
                            />
                            <AttendanceActionButton
                                status={!shift.checkInAt ? "IN" : "OUT"}
                                isLoading={isSubmitting || isLocating}
                                onPress={onMainPress}
                            />
                        </>
                    ) : (
                        /* ẨN: Hiện thông báo nhắc nhở khi ngoài giờ */
                        <div className="flex flex-col items-center gap-2 py-4 px-6 bg-default-50 rounded-2xl border border-dashed border-default-300">
                            <Info size={20} className="text-default-400" />
                            <p className="text-sm text-default-500 font-medium text-center leading-relaxed">
                                {message}
                            </p>
                        </div>
                    )}
                </CardBody>
            </Card>

            {/* Modal giải trình khi có sự cố GPS */}
            <ExceptionNoteModal
                isOpen={isOpen}
                onClose={onClose}
                title={attendanceStatus === "IN" ? "Vào ca" : "Tan ca"}
                isLoading={isSubmitting}
                onSubmit={onExceptionSubmit}
            />
        </>
    )
}
