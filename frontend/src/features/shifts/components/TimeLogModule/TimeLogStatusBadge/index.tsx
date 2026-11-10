import React from "react"
import { Chip } from "@heroui/react"
import { TimeLogStatus } from "@/features/shifts/constants"
import { TimeLogStatusStyles } from "@/features/shifts/constants/styles"
import { TimeLogStatusLabels } from "@/features/shifts/constants/labels"

interface Props {
    status: TimeLogStatus
    className?: string
}

export const TimeLogStatusBadge = ({ status, className }: Props) => {
    return (
        <Chip
            size="sm"
            variant="dot"
            classNames={{
                base: `${TimeLogStatusStyles[status]} px-2 h-6 ${className}`,
                content: "font-bold text-[11px] uppercase tracking-tight",
                dot: "hidden"
            }}
        >
            {TimeLogStatusLabels[status]}
        </Chip>
    )
}
