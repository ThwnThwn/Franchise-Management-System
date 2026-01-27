"use client"
import { AutocompleteItem } from "@heroui/react"
import React from "react"
import type { Key } from "@react-types/shared"
import { AutocompleteStyled } from "@/components"

type EnumPickerProps<T extends number> = {
    value?: Key | null
    onChange?: (value: Key | null) => void
    onKeyDown?: () => void
    labels: Record<T, string> // map enum → label (ví dụ OrderStatusLabels)
    label?: string // nhãn hiển thị cho Autocomplete
    className?: string
    isReadOnly?: boolean
    isClearable?: boolean
    isRequired?: boolean
}

export function EnumPicker<T extends number>({
    value,
    onChange,
    onKeyDown = undefined,
    labels,
    label,
    className,
    isReadOnly = false,
    isClearable = true,
    isRequired = false
}: EnumPickerProps<T>) {
    // Lấy items từ labels
    const items = Object.entries(labels).map(([key, label]) => ({
        key,
        label
    }))

    return (
        <AutocompleteStyled
            label={label}
            isReadOnly={isReadOnly}
            isClearable={isClearable}
            variant="bordered"
            className={className}
            selectedKey={value !== null ? String(value) : null}
            onSelectionChange={onChange}
            onKeyDown={onKeyDown}
            classNames={{
                popoverContent: "min-w-fit"
            }}
            isRequired={isRequired}
        >
            {items.map((item) => (
                <AutocompleteItem key={item.key}>{item.label as React.ReactNode}</AutocompleteItem>
            ))}
        </AutocompleteStyled>
    )
}
