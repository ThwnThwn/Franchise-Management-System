"use client"
import React from "react"
import { cn, Skeleton, SkeletonProps } from "@heroui/react"

// Skeleton cho đoạn text (p)
export function TextSkeleton(props: SkeletonProps) {
    return <Skeleton {...props} className={cn("h-3 w-[60vh] rounded-lg", props.className)} />
}

export function CircleSkeleton(props: SkeletonProps) {
    return <Skeleton {...props} className={cn("rounded-full", props.className)} />
}

// Skeleton cho title (heading)
export function TitleSkeleton(props: SkeletonProps) {
    return <Skeleton {...props} className={cn("h-30 w-[60vh] rounded-lg", props.className)} />
}
