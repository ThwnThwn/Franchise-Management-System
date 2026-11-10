"use client"

import React, { useMemo, useState, useEffect } from "react"
import { Card, CardBody, CardHeader, Divider, useDisclosure } from "@heroui/react"
import { Store, CalendarDays, Info } from "lucide-react"
import { useAttendanceActions, useAttendanceWatcher } from "@/features/shifts/hooks"
import { useDay, useDistanceToTarget, useGeolocation } from "@/hooks"
import { getAttendanceUIConfig } from "@/features/shifts/utils/ui-utils"
import { ShiftSkeleton } from "./ShiftSkeleton"
import { AttendanceStatus } from "./AttendanceStatus"
import { ShiftCountDown } from "./ShiftCountDown"
import { AttendanceActionButton } from "./AttendanceActionButton"
import { ExceptionNoteModal } from "@/features/shifts/components"
import dayjs from "dayjs"
import { getShiftDateTime } from "@/features/shifts/utils"

export const CurrentShiftCard = () => {
    const { isOpen, onOpen, onClose } = useDisclosure()
    const { formatDate } = useDay()

    const { status, isLoading: isWatcherLoading, refetch } = useAttendanceWatcher()
    const shift = status?.shift

    // Lấy cấu hình UI (Màu sắc, Icon, Label) đồng bộ với Floating Bar
    const ui = useMemo(() => getAttendanceUIConfig(status), [status])

    // Logic hiển thị nội dung (Quyết định xem hiện Nút hay hiện Thông báo)
    const display = useMemo(() => {
        if (!status || !shift) return { canAction: false, message: "" }

        // Giai đoạn: CHƯA VÀO CA
        if (!shift.checkInAt) {
            if (
                status.isLate ||
                status.isComingUp ||
                (dayjs().isAfter(dayjs(`${dayjs().format("YYYY-MM-DD")} ${shift.startTime}`)) &&
                    status.deadlineMinutes > 0)
            ) {
                return { canAction: true, type: "IN" }
            }
            if (status.deadlineMinutes < 0) {
                return {
                    canAction: false,
                    message: "Ca làm việc đã kết thúc, bạn đã bỏ lỡ Check-in."
                }
            }
            return { canAction: false, message: `Hệ thống chưa mở cổng Check-in cho ca này.` }
        }

        // Giai đoạn: ĐANG TRONG CA
        if (status.isOngoing) {
            if (status.deadlineMinutes < 0) {
                return { canAction: false, message: "Đã quá thời gian cho phép Check-out tự động." }
            }
            // Check-out theo Window (Dựa vào canCheckOut của hook đã tính Grace Period)
            if (!status.canCheckOut) {
                return { canAction: false, message: "Chưa đến khung giờ được phép Check-out." }
            }
            return { canAction: true, type: "OUT" }
        }

        return { canAction: false, message: "Bạn đã hoàn thành ca làm việc hôm nay." }
    }, [status, shift])

    const { startTime, endTime } = useMemo(() => {
        if (!shift) return { startTime: dayjs(), endTime: dayjs() }

        const todayStr = formatDate(dayjs())

        const startObj = dayjs(getShiftDateTime(todayStr, shift.startTime))
        const endObj = dayjs(getShiftDateTime(todayStr, shift.endTime, shift.startTime))

        return { startTime: startObj, endTime: endObj }
    }, [shift, formatDate])

    // Vị trí địa lý (Hardware logic)
    const { getPosition, isLoading: isLocating } = useGeolocation()
    const [coords, setCoords] = useState<{ lat: number; lng: number } | null>(null)
    const { distanceM, isWithinRange } = useDistanceToTarget(
        coords,
        shift?.storeLat && shift?.storeLong ? { lat: shift.storeLat, lng: shift.storeLong } : null
    )

    // Hành động Chấm công
    const { handleAction, isLoading: isSubmitting } = useAttendanceActions()

    const onMainPress = async () => {
        const pos = await getPosition()
        if (pos.lat !== undefined && pos.lng !== undefined)
            setCoords({ lat: pos.lat, lng: pos.lng })

        if (pos.error || (distanceM && !isWithinRange)) {
            onOpen() // Sai vị trí -> Mở modal giải trình
            return
        }

        await handleAction({
            type: status?.isOngoing ? "OUT" : "IN",
            id: (status?.isOngoing ? shift?.currentLogId : shift?.assignmentId) || "",
            target: { lat: shift?.storeLat || 0, lng: shift?.storeLong || 0 }
        })
        refetch() // Cập nhật lại status mới
    }

    useEffect(() => {
        if (shift?.hasShiftToday) {
            getPosition().then((p) => p.lat && p.lng && setCoords({ lat: p.lat, lng: p.lng }))
        }
    }, [shift?.hasShiftToday, getPosition])

    // RENDER STATES
    if (isWatcherLoading) return <ShiftSkeleton />

    if (!shift?.hasShiftToday || status === undefined)
        return (
            <Card className="bg-default-50 border-none p-8 text-center text-default-600 font-medium">
                Hôm nay bạn không có ca làm việc nào.
            </Card>
        )

    return (
        <>
            <Card className="shadow-sm border border-default-200 overflow-hidden">
                <CardHeader className="flex justify-between items-start p-5 bg-content1">
                    <div className="flex gap-3">
                        <div
                            className={`p-3 ${ui?.hexColor || "bg-primary"} rounded-2xl text-white shadow-lg transition-colors`}
                        >
                            <Store size={24} />
                        </div>
                        <div className="flex flex-col">
                            <span className="text-lg font-bold text-default-800">
                                {shift.storeName}
                            </span>
                            <AttendanceStatus
                                distanceM={distanceM}
                                isWithinRange={isWithinRange}
                                isLoading={isLocating}
                            />
                        </div>
                    </div>
                    <div className="flex flex-col items-end">
                        <div className="flex items-center gap-1 text-default-400 text-tiny uppercase font-bold tracking-wider">
                            <CalendarDays size={14} /> Hôm nay
                        </div>
                        <span className="font-bold text-default-700">
                            {shift.startTime} - {shift.endTime}
                        </span>
                    </div>
                </CardHeader>

                <Divider />

                <CardBody className="p-6 flex flex-col gap-4">
                    {display.canAction ? (
                        <>
                            <ShiftCountDown
                                targetTime={status.isOngoing ? endTime : startTime}
                                label={
                                    status.isOngoing
                                        ? "Thời gian kết thúc ca"
                                        : "Thời gian ca bắt đầu"
                                }
                            />
                            <AttendanceActionButton
                                status={status.isOngoing ? "OUT" : "IN"}
                                isLoading={isSubmitting || isLocating}
                                onPress={onMainPress}
                            />
                        </>
                    ) : (
                        /* ẨN: Hiện thông báo nhắc nhở khi ngoài giờ */
                        <div className="flex flex-col items-center gap-2 py-4 px-6 bg-default-50 rounded-2xl border border-dashed border-default-300">
                            <Info size={20} className="text-default-400" />
                            <p className="text-sm text-default-500 font-medium text-center leading-relaxed">
                                {display.message}
                            </p>
                        </div>
                    )}
                </CardBody>
            </Card>

            <ExceptionNoteModal
                isOpen={isOpen}
                onClose={onClose}
                title={status.isOngoing ? "Giải trình Tan ca" : "Giải trình Vào ca"}
                isLoading={isSubmitting}
                onSubmit={async (note) => {
                    await handleAction({
                        type: status.isOngoing ? "OUT" : "IN",
                        id: status.isOngoing ? shift.currentLogId! : shift.assignmentId!,
                        userNote: note
                    })
                    onClose()
                    refetch()
                }}
            />
        </>
    )
}
