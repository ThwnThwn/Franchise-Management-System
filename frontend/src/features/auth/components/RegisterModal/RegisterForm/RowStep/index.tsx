"use client"
import { AUTH_TEXT } from "@/features/auth/constants"
import RowSteps from "@/components/modules/RowSteps"
import React from "react"

interface RowStepProps {
    n: number // chỉ nhận giá trị step từ cha
}
export function RowStep({ n }: RowStepProps) {
    const steps = [{ title: AUTH_TEXT.TITLES.REGISTER }, { title: AUTH_TEXT.TITLES.CONFIRM_OTP }]

    return (
        <div className="flex justify-center">
            <RowSteps currentStep={n} steps={steps} />
        </div>
    )
}
