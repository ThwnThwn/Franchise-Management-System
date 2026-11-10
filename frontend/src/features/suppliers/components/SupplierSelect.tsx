"use client"

import { Select, SelectItem, SharedSelection } from "@heroui/react"
import { useGetSuppliers } from "../hooks/useSuppliers"
import { SupplierItem } from "../models/response"

interface SupplierSelectProps {
    value?: string
    onSelectionChange?: (keys: SharedSelection) => void
    label?: string
    placeholder?: string
    errorMessage?: string
    isInvalid?: boolean
    isRequired?: boolean
    className?: string
}

export function SupplierSelect({
    value,
    onSelectionChange,
    label = "Nhà cung cấp",
    placeholder = "Chọn nhà cung cấp",
    errorMessage,
    isInvalid,
    isRequired,
    className
}: SupplierSelectProps) {
    const { data, isLoading } = useGetSuppliers({
        params: { isActive: true, page: 1, pageSize: 100 }
    })

    const suppliers = data?.items ?? []

    return (
        <Select
            label={label}
            placeholder={placeholder}
            selectedKeys={value ? [value] : []}
            onSelectionChange={onSelectionChange}
            isLoading={isLoading}
            errorMessage={errorMessage}
            isInvalid={isInvalid}
            isRequired={isRequired}
            className={className}
            variant="bordered"
            radius="lg"
        >
            {suppliers.map((supplier: SupplierItem) => (
                <SelectItem key={supplier.id} textValue={supplier.name}>
                    <div className="flex flex-col">
                        <span className="text-small">{supplier.name}</span>
                        <span className="text-tiny text-default-400">{supplier.code}</span>
                    </div>
                </SelectItem>
            ))}
        </Select>
    )
}
