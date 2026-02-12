"use client"

import React, { useState } from "react"
import { Card, CardBody, CardHeader, Divider, useDisclosure } from "@heroui/react"
import { Store, CalendarDays } from "lucide-react"

import { useGetCurrentShift, useAttendanceActions } from "@/features/shifts/hooks"
import { useDistanceToTarget, useGeolocation } from "@/hooks"
import { ShiftSkeleton } from "./ShiftSkeleton"
import { AttendanceStatus } from "./AttendanceStatus"
import { ShiftCountDown } from "./ShiftCountDown"
import { AttendanceActionButton } from "./AttendanceActionButton"
import { ExceptionNoteModal } from "@/features/shifts/components"

export const CurrentShiftCard = () => {
    const { isOpen, onOpen, onClose } = useDisclosure()

    // 1. Data Fetching
    const { data: shift, isLoading: isFetchingShift } = useGetCurrentShift()

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

    if (isFetchingShift) return <ShiftSkeleton />

    if (!shift?.hasShiftToday)
        return (
            <Card className="bg-default-50 border-none p-6 text-center text-default-400">
                Hôm nay bạn không có ca làm việc nào.
            </Card>
        )

    return (
        <>
            <Card className="shadow-sm border border-default-200">
                <CardHeader className="flex justify-between items-start p-5">
                    <div className="flex gap-3">
                        <div className="p-3 bg-primary-100 rounded-2xl text-primary">
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

                <CardBody className="p-5 flex flex-col gap-6">
                    {/* Đồng hồ đếm ngược (Chỉ hiện khi đang trong ca hoặc sắp vào ca) */}
                    {attendanceStatus !== "DONE" && (
                        <ShiftCountDown
                            targetTime={
                                attendanceStatus === "IN" ? shift.serverTime : shift.endTime
                            }
                            label={
                                attendanceStatus === "IN"
                                    ? "Thời gian hiện tại"
                                    : "Thời gian kết thúc ca"
                            }
                        />
                    )}

                    {/* Nút bấm hành động chính */}
                    <AttendanceActionButton
                        status={attendanceStatus as any}
                        isLoading={isSubmitting || isLocating}
                        onPress={onMainPress}
                    />
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
