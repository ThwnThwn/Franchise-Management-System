"use client"
import { Table, TableProps, cn } from "@heroui/react"
import React from "react"

export function TableStyled({ className, children, ...rest }: TableProps) {
    return (
        <Table
            isHeaderSticky
            {...rest}
            className={cn("overflow-auto w-full", className)}
            classNames={{
                ...rest.classNames,
                wrapper: cn("p-0", rest.classNames?.wrapper),
                emptyWrapper: cn("h-fit py-3", rest.classNames?.emptyWrapper)
            }}
        >
            {children}
        </Table>
    )
}
