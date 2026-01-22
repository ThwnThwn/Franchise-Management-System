"use client"
import React from "react"
import type { Key } from "@react-types/shared"
import {
    TableHeader,
    TableColumn,
    TableBody,
    TableRow,
    TableCell,
    getKeyValue,
    cn
} from "@heroui/react"
import { TableStyled } from "@/components"
import { STRINGS } from "@/constants/strings"

export type Column = { key: string; label: string }
export type Row = Record<string, any>

export type TableSelectionStyledProps = {
    columns: Column[]
    rows: Row[]
    className?: string
    selectedKeys?: "all" | Iterable<Key>
    selectionBehavior?: "toggle" | "replace"
    onSelectionChange?: (keys: Key[]) => void
}

export function TableSelectionStyled({
    columns,
    rows,
    className,
    selectedKeys,
    selectionBehavior,
    onSelectionChange
}: TableSelectionStyledProps) {
    return (
        <div className={cn("flex flex-col gap-3", className)}>
            <TableStyled
                selectionBehavior={selectionBehavior}
                selectionMode="multiple"
                selectedKeys={selectedKeys}
                onSelectionChange={(keys) => {
                    let selected: Key[]

                    if (keys === "all") {
                        selected = rows.map((r) => r.key)
                    } else {
                        selected = Array.from(keys)
                    }

                    onSelectionChange?.(selected)
                }}
            >
                <TableHeader columns={columns}>
                    {(column) => <TableColumn key={column.key}>{column.label}</TableColumn>}
                </TableHeader>
                <TableBody items={rows} emptyContent={STRINGS.TABLE.EMPTY}>
                    {(item) => (
                        <TableRow key={item.key}>
                            {(columnKey) => <TableCell>{getKeyValue(item, columnKey)}</TableCell>}
                        </TableRow>
                    )}
                </TableBody>
            </TableStyled>
        </div>
    )
}
