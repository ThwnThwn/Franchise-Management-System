"use client"

import React from "react"
import { AutocompleteItem, AutocompleteProps } from "@heroui/react"
import { AutocompleteStyled } from "@/components"

// Loại bỏ các props mình muốn ghi đè logic riêng để tránh xung đột
interface EnumPickerProps<T extends number> extends Omit<
    AutocompleteProps,
    "value" | "onChange" | "children" | "items" | "onSelectionChange" | "selectedKey"
> {
    labels: Record<T, string>
    value?: T | null
    onChange?: (value: T | null) => void
}

export function EnumPicker<T extends number>({
    labels,
    value,
    onChange,
    ...restProps // Gom tất cả props còn lại của HeroUI (label, placeholder, isReadOnly, etc.)
}: EnumPickerProps<T>) {
    // Convert object enum thành mảng để render
    const items = Object.entries(labels).map(([key, label]) => ({
        key: String(key),
        label: label as string
    }))

    const handleSelectionChange = (key: React.Key | null) => {
        if (!onChange) return
        // Trả về kiểu số (number) để khớp với Enum của bro thay vì string
        onChange(key !== null ? (Number(key) as T) : null)
    }

    return (
        <AutocompleteStyled
            variant="bordered"
            {...restProps} // Spread props: giờ bro truyền cái gì vào EnumPicker nó cũng bay thẳng vào đây
            selectedKey={value !== null && value !== undefined ? String(value) : null}
            onSelectionChange={handleSelectionChange}
            classNames={{
                popoverContent: "min-w-fit",
                ...restProps.classNames // Cho phép ghi đè classNames từ bên ngoài
            }}
        >
            {items.map((item) => (
                <AutocompleteItem key={item.key}>{item.label}</AutocompleteItem>
            ))}
        </AutocompleteStyled>
    )
}
