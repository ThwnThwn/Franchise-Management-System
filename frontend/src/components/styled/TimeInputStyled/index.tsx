"use client"
import { TimeInput, TimeInputProps, cn } from "@heroui/react"
import React from "react"

interface TimeInputStyledProps extends TimeInputProps {}

export function TimeInputStyled(props: TimeInputStyledProps) {
    return (
        <TimeInput
            variant="bordered"
            hourCycle={24} // Mặc định 24h
            {...props}
            classNames={{
                ...props.classNames,
                inputWrapper: cn(
                    // Style border khi hover/focus giống InputStyled
                    "data-[hover=true]:border-primary data-[focus-within=true]:border-primary",
                    "group-data-[focus=true]:border-primary", // Hỗ trợ thêm state group
                    props.classNames?.inputWrapper
                ),
                label: cn("text-gray-700", props.classNames?.label),
                // TimeInput dùng "segment" để hiển thị số giờ/phút thay vì "input"
                segment: cn(
                    "text-gray-900 text-base", // Màu chữ và size
                    "data-[placeholder=true]:text-default-400", // Màu placeholder nếu có
                    props.classNames?.segment
                )
            }}
        />
    )
}
