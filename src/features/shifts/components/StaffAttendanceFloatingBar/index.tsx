"use client"

import React, { useMemo } from "react"
import { Card, Button, useDisclosure } from "@heroui/react"
import { AlertCircle, Clock, LogIn } from "lucide-react"
import { useAttendanceWatcher, useAttendanceActions } from "@/features/shifts/hooks"
import { useGeolocation } from "@/hooks"
import { ExceptionNoteModal } from "@/features/shifts/components"

export const StaffAttendanceFloatingBar = () => {
    const { status, isLoading: isWatcherLoading } = useAttendanceWatcher()
    const { handleAction, isLoading: isSubmitting } = useAttendanceActions()
    const { getPosition, isLoading: isLocating } = useGeolocation()
    const { isOpen, onOpen, onClose } = useDisclosure()

    // 1. Logic lấy UI config (Giống bản trước)
    const uiConfig = useMemo(() => {
        if (!status) return null
        if (status.isLate)
            return { color: "bg-danger", icon: <AlertCircle size={20} />, label: "Vào ca ngay" }
        if (status.isOvertime)
            return { color: "bg-warning", icon: <Clock size={20} />, label: "Tan ca ngay" }
        return {
            color: "bg-secondary",
            icon: <LogIn size={20} />,
            label: status.isOngoing ? "Tan ca" : "Vào ca"
        }
    }, [status])

    // 2. Handler xử lý nhanh
    const handleQuickAction = async () => {
        if (!status) return

        const pos = await getPosition()

        const target = {
            lat: status.shift.storeLat || 0,
            lng: status.shift.storeLong || 0
        }

        if (pos.error || !pos.lat) {
            onOpen()
            return
        }

        await handleAction({
            type: status.isOngoing ? "OUT" : "IN",
            id: (status.isOngoing ? status.shift.currentLogId : status.shift.assignmentId) || "",
            target
        })
    }

    if (isWatcherLoading || !status?.shouldWarn || !uiConfig) return null

    return (
        <>
            <div className="fixed w-md bottom-6 left-11 right-0 px-4 z-99 animate-in slide-in-from-bottom-5">
                <Card className={`${uiConfig.color} border-white border-2 shadow-2xl`}>
                    <div className="flex items-center justify-between p-3">
                        <div className="flex items-center gap-3">
                            <div className="p-2 bg-white/20 rounded-lg">{uiConfig.icon}</div>
                            <div className="flex flex-col text-white">
                                <span className="text-xs opacity-80">{status.shift.storeName}</span>
                                <span className="text-sm font-bold leading-tight">
                                    {status.isOngoing ? "Đang làm việc..." : "Sẵn sàng vào ca"}
                                </span>
                            </div>
                        </div>

                        <Button
                            size="sm"
                            isLoading={isSubmitting || isLocating}
                            className="bg-white text-black font-extrabold px-6"
                            onPress={handleQuickAction}
                        >
                            {uiConfig.label}
                        </Button>
                    </div>
                </Card>
            </div>

            {/* Vẫn cần Modal này để fallback nếu GPS có vấn đề */}
            <ExceptionNoteModal
                isOpen={isOpen}
                onClose={onClose}
                title={status.isOngoing ? "Tan ca" : "Vào ca"}
                isLoading={isSubmitting}
                onSubmit={async (note) => {
                    await handleAction({
                        type: status.isOngoing ? "OUT" : "IN",
                        id: status.isOngoing
                            ? status.shift.currentLogId!
                            : status.shift.assignmentId!,
                        userNote: note
                    })
                    onClose()
                }}
            />
        </>
    )
}
