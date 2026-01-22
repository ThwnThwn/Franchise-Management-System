"use client"
import { cn, Dropdown, DropdownProps } from "@heroui/react"
import React from "react"

type DropdownStyleProps = DropdownProps & {
    children: React.ReactNode
}

export function DropdownStyled({ children, className, ...props }: DropdownStyleProps) {
    return (
        <Dropdown
            placement="bottom-start"
            {...props}
            className={cn("font-medium text-base", className)}
        >
            {children}
        </Dropdown>
    )
}
