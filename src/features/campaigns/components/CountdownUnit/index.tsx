"use client"

import { cn } from "@heroui/react"

interface CountdownUnitProps {
    value: number
    label: string
    isSeconds?: boolean
    className?: string
    labelClassName?: string
    valueClassName?: string
}

export const CountdownUnit = ({
    value,
    label,
    isSeconds,
    className,
    labelClassName,
    valueClassName
}: CountdownUnitProps) => (
    <div className={cn("text-center min-w-8", className)}>
        <span
            suppressHydrationWarning
            className={cn("text-2xl font-black", isSeconds && "text-danger-500", valueClassName)}
        >
            {value.toString().padStart(2, "0")}
        </span>
        <p className={cn("text-[9px] uppercase font-bold opacity-70 mt-1", labelClassName)}>
            {label}
        </p>
    </div>
)
