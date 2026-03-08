"use client"

import { User } from "@heroui/react"

import { StaffAssignmentCandidate } from "@/features/shifts/models/response"
import { useUserHelper } from "@/hooks"
import { getValidationErrors } from "@/features/shifts/utils"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { WarningTooltip } from "../WarningTooltip"

// 2. COMPONENT: Giao diện dòng User (Tái sử dụng cho renderValue và SelectItem)
export const CandidateOptionUI = ({
    candidate,
    placement,
    className = ""
}: {
    candidate: StaffAssignmentCandidate
    placement: "top" | "left"
    className?: string
}) => {
    const { toFullName } = useUserHelper()
    const errors = getValidationErrors(candidate)

    return (
        <div className={`flex justify-between items-center w-full ${className}`}>
            <User
                name={toFullName(candidate)}
                description={`${formatCurrencyWithSymbol(candidate.currentHourlyWage)}/h`}
                avatarProps={{ src: candidate.avatarUrl, size: "sm" }}
            />
            {errors.length > 0 && <WarningTooltip errors={errors} placement={placement} />}
        </div>
    )
}
