"use client"
import { LOGO_URL } from "@/constants/constants"
import { cn } from "@heroui/react"
import Image, { ImageProps } from "next/image"
import React from "react"

type LogoImageProps = Omit<ImageProps, "src" | "alt"> & {
    src?: ImageProps["src"]
    alt?: string
}

export function LogoStyled({ src = LOGO_URL, alt = "LOGO", ...rest }: LogoImageProps) {
    return (
        <Image
            width={60}
            height={60}
            {...rest}
            src={src}
            alt={alt}
            className={cn("w-20 h-20 rounded-full bg-black", rest.className)}
        />
    )
}
