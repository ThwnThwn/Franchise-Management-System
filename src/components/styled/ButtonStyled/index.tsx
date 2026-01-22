"use client"
import React from "react"
import { Button, ButtonProps, cn, Spinner } from "@heroui/react"

export function ButtonStyled({ className, color = "default", isLoading, ...rest }: ButtonProps) {
    if (isLoading) {
        return <Spinner />
    }

    return <Button color={color} {...rest} className={cn("font-medium text-base", className)} />
}
