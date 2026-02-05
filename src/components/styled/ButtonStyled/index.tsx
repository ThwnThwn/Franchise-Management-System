"use client"
import React from "react"
import { Button, ButtonProps, cn } from "@heroui/react"

// Tạo một interface mới loại bỏ các trường gây xung đột từ ButtonProps gốc
interface ButtonStyledProps extends Omit<ButtonProps, "className" | "color" | "isLoading"> {
    className?: string
    color?: ButtonProps["color"]
    isLoading?: boolean
    children?: React.ReactNode
}

export function ButtonStyled({
    className,
    color = "default",
    isLoading,
    children,
    ...rest
}: ButtonStyledProps) {
    return (
        <Button
            color={color}
            isLoading={isLoading}
            {...(rest as any)} // Cast nhẹ về any ở đây để cắt đứt chuỗi logic phức tạp của TS
            className={cn("font-medium text-base", className) as string}
        >
            {children}
        </Button>
    )
}
