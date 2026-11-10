"use client"

import React, { useMemo } from "react"
import { AutocompleteItem, AutocompleteProps } from "@heroui/react"
import { AutocompleteStyled } from "@/components"

interface EnumPickerProps<T extends string | number> extends Omit<
    AutocompleteProps,
    "value" | "onChange" | "children" | "items" | "onSelectionChange" | "selectedKey"
> {
    labels: Partial<Record<T, string>>
    value?: T
    onChange?: (value?: T) => void
}

export function EnumPicker<T extends string | number>({
    labels,
    value,
    onChange,
    disabledKeys,
    ...restProps
}: EnumPickerProps<T>) {
    // 1. Chuyển đổi labels thành mảng items
    const items = useMemo(() => {
        return Object.entries(labels)
            .filter(([, label]) => label !== undefined)
            .map(([key, label]) => ({
                key: String(key),
                label: label as string
            }))
    }, [labels])

    // 2. Tự động suy luận kiểu Enum dựa trên key đầu tiên của labels
    const isActuallyNumberEnum = useMemo(() => {
        const keys = Object.keys(labels)
        if (keys.length === 0) return false

        // Kiểm tra xem key đầu tiên có phải là "chuỗi số" không (VD: "0", "1")
        // Lưu ý: Chúng ta dùng !isNaN để check, vì Enum string như "admin" sẽ ra NaN
        const firstKey = keys[0]
        return !isNaN(Number(firstKey)) && isFinite(Number(firstKey))
    }, [labels])

    const stringDisabledKeys = useMemo(() => {
        if (!disabledKeys) return []
        return Array.from(disabledKeys).map((key) => String(key))
    }, [disabledKeys])

    const handleSelectionChange = (key: React.Key | null) => {
        if (!onChange) return

        if (key === null) {
            onChange(undefined)
            return
        }

        const stringKey = String(key)

        // Nếu tự suy luận là Number Enum thì convert, không thì giữ String
        const finalValue = isActuallyNumberEnum ? (Number(stringKey) as T) : (stringKey as T)

        onChange(finalValue)
    }

    return (
        <AutocompleteStyled
            variant="bordered"
            {...restProps}
            selectedKey={value !== undefined ? String(value) : null}
            onSelectionChange={handleSelectionChange}
            classNames={{
                popoverContent: "min-w-fit",
                ...restProps.classNames
            }}
            disabledKeys={stringDisabledKeys}
        >
            {items.map((item) => (
                <AutocompleteItem key={item.key}>{item.label}</AutocompleteItem>
            ))}
        </AutocompleteStyled>
    )
}
