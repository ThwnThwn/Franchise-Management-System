"use client"

import React from "react"
import { ButtonStyled } from "@/components"
import { LogIn, LogOut, CheckCircle2 } from "lucide-react"
import { confirmDialog } from "@/hooks"

interface AttendanceActionButtonProps {
    status: "IN" | "OUT" | "DONE"
    isLoading: boolean
    onPress: () => void
}

export const AttendanceActionButton = ({
    status,
    isLoading,
    onPress
}: AttendanceActionButtonProps) => {
    if (status === "DONE")
        return (
            <ButtonStyled isDisabled color="success" variant="flat" className="w-full h-14 text-lg">
                <CheckCircle2 size={20} /> Hoàn thành ca làm
            </ButtonStyled>
        )

    const isCheckIn = status === "IN"

    const handleOnPress = async () => {
        const isConfirmed = await confirmDialog({
            message: `Bạn muốn ${isCheckIn ? "CHECK-IN" : "CHECK-OUT"} ca làm việc?`,
            variant: "info"
        })
        if (isConfirmed) onPress()
    }

    return (
        <ButtonStyled
            color={isCheckIn ? "primary" : "secondary"}
            className="w-full shadow-lg"
            isLoading={isLoading}
            onPress={handleOnPress}
            startContent={isCheckIn ? <LogIn size={22} /> : <LogOut size={22} />}
        >
            {isCheckIn ? "VÀO CA NGAY (CHECK-IN)" : "TAN CA (CHECK-OUT)"}
        </ButtonStyled>
    )
}
