"use client"

import { Avatar, AvatarProps } from "@heroui/react"

export function AvatarStyled(props: AvatarProps) {
    return (
        <Avatar
            isBordered
            // className="min-w-30 min-h-30"
            {...(props as any)}
            // className={cn("font-medium text-base", props.className)}
        />
    )
}
