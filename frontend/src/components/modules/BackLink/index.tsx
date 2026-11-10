"use client"

import React from "react"
import NextLink from "next/link"
import { Link } from "@heroui/react"
import { ChevronLeft } from "lucide-react"
import { useRouter } from "next/navigation"

interface BackLinkProps {
    href?: string
    label?: string
    className?: string
    showIcon?: boolean
}

export const BackLink = ({
    href,
    label = "Quay lại",
    className = "",
    showIcon = true
}: BackLinkProps) => {
    const router = useRouter()

    const handlePress = () => {
        if (!href) {
            router.back()
        }
    }

    return (
        <Link
            // Nếu không có href, ta ép nó về tag button để tránh lỗi thẻ <a> trống
            as={href ? NextLink : "button"}
            href={href}
            onPress={handlePress}
            className={`gap-1.5 font-bold h-fit transition-opacity hover:opacity-80 active:scale-95 ${className}`}
            color="foreground"
        >
            {showIcon && <ChevronLeft size={16} strokeWidth={2.5} />}
            {label}
        </Link>
    )
}
