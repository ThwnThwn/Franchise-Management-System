import React from "react"
import Image from "next/image"
import { cn } from "@heroui/react"
import { TitleSkeleton } from "../SkeletonStyled"
import { getPixelSizeFromClass } from "@/utils/helpers/uiHelper"

export function ImageStyled({
    src,
    alt,
    className
}: {
    src?: string
    alt: string
    className?: string
}) {
    const { width, height } = getPixelSizeFromClass(className)

    if (!src) return <TitleSkeleton className={className} />
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
