"use client"
import RowSteps from "@/Modules/RowSteps"
import React from "react"

interface RowStepProps {
    n: number // chỉ nhận giá trị step từ cha
}
export function RowStep({ n }: RowStepProps) {
    const steps = [{ title: "auth.register" }, { title: "auth.confirm_otp" }]

    return (
        <div className="flex justify-center">
            <RowSteps currentStep={n} steps={steps} />
        </div>
    )
}
