import React from "react"
import Image from "next/image"
import { cn } from "@heroui/react"
import { TitleSkeleton } from "../SkeletonStyled"

export function ImageStyled({
    src,
    alt,
    className,
    width,
    height
}: {
    src?: string
    alt: string
    className?: string
    width?: number
    height?: number
}) {
    if (!src) return <TitleSkeleton className={`w-[${width}px] h-[${height}px]`} />
    return (
        <Image
            alt={alt}
            src={src}
            className={cn("object-cover rounded-xl", className)}
            width={width || 500}
            height={height || 500}
        />
    )
}
