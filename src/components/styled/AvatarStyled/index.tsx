"use client"

import React from "react"
import { Avatar, AvatarProps } from "@heroui/react"

export function AvatarStyled(props: AvatarProps) {
    return (
        <Avatar
            isBordered
            // className="min-w-30 min-h-30"
            {...props}
            // className={cn("font-medium text-base", props.className)}
        />
    )
}
