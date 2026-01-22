"use client"
import { cn, DateRangePicker, DateRangePickerProps } from "@heroui/react"
import React from "react"
export function DateRangePickerStyled(props: DateRangePickerProps) {
    return (
        <DateRangePicker
            showMonthAndYearPickers
            variant="bordered"
            {...props}
            className={cn("w-[240] font-medium text-base", props.className)}
            classNames={{
                ...props.classNames,
                inputWrapper: cn(
                    "hover:border-primary focus-within:border-primary focus-within:hover:border-primary",
                    props.classNames?.inputWrapper
                ),
                label: cn("text-gray-700 text-left", props.classNames?.label),
                input: cn("text-gray-900", props.classNames?.input)
            }}
        />
    )
}
