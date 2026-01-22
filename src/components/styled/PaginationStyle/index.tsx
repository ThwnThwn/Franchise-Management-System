"use client"

import React from "react"
import { Pagination, PaginationProps, cn } from "@heroui/react"

export function PaginationStyled({ page, onChange, className, ...rest }: PaginationProps) {
    const handleClick = (value: number) => {
        onChange?.(value)
    }

    return (
        <Pagination
            hidden={rest.total <= 0}
            {...rest}
            page={page}
            onChange={handleClick}
            className={cn("flex justify-center items-center", className)}
        />
    )
}
