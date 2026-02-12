"use client"

import React from "react"
import { ButtonStyled } from "@/components"
import { LogIn, LogOut, CheckCircle2 } from "lucide-react"

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

    return (
        <ButtonStyled
            color={isCheckIn ? "primary" : "secondary"}
            className="w-full h-14 text-lg font-bold shadow-lg"
            isLoading={isLoading}
            onPress={onPress}
            startContent={isCheckIn ? <LogIn size={22} /> : <LogOut size={22} />}
        >
            {isCheckIn ? "VÀO CA NGAY" : "TAN CA (CHECK-OUT)"}
        </ButtonStyled>
    )
}
