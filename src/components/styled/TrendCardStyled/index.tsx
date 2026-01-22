"use client"
import { Chip, cn } from "@heroui/react"
import React from "react"
import { CardStyled } from "../CardStyled"
import { Icon } from "@iconify/react"
import { Percent } from "lucide-react"
import { formatCurrency } from "@/utils/helpers/currency"

const trendIcons = {
    up: "solar:arrow-right-up-linear",
    neutral: "solar:arrow-right-linear",
    down: "solar:arrow-right-down-linear"
}

const trendColors = {
    positive: "success",
    neutral: "warning",
    negative: "danger"
} as const

export type TrendCardProps = {
    title: string
    value: number
    change: string | number
    changeType: "positive" | "neutral" | "negative"
    trendType: "up" | "neutral" | "down"
    trendChipPosition?: "top" | "bottom"
    trendChipVariant?: "flat" | "light" | "solid" | "bordered" | "faded" | "shadow" | "dot"
}

export function TrendCardStyled({
    title,
    value,
    change,
    changeType,
    trendType,
    trendChipPosition = "top",
    trendChipVariant = "flat"
}: TrendCardProps) {
    return (
        <CardStyled className="dark:border-default-100 border border-transparent">
            <div className="flex p-4 relative justify-between items-start">
                <div className="flex flex-col gap-y-2">
                    <dt className="text-small text-default-500 font-medium">{title}</dt>
                    <dd className="text-default-700 text-2xl font-semibold">
                        {formatCurrency(value)}
                    </dd>
                </div>

                <Chip
                    className={cn("absolute right-4", {
                        "top-4": trendChipPosition === "top",
                        "bottom-4": trendChipPosition === "bottom"
                    })}
                    classNames={{
                        content: "font-medium text-[0.65rem]"
                    }}
                    color={trendColors[changeType]}
                    radius="sm"
                    size="sm"
                    startContent={<Icon height={12} width={12} icon={trendIcons[trendType]} />}
                    variant={trendChipVariant}
                >
                    <span className="flex items-center gap-0.5">
                        {change}

                        <Percent size={10} strokeWidth={3} />
                    </span>
                </Chip>
            </div>
        </CardStyled>
    )
}
