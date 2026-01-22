import React from "react"
import CardReview from "./CardReview"
import { cn } from "@heroui/react"

type CardReviewProps = {
    name: string
    avatar?: string
    rating: number
    station: string
    content: string
    createdAt: string
    className?: string
    isDeleteable?: boolean
    onDelete?: () => void
}

export default function CardReviewUser({
    name,
    avatar,
    rating,
    station,
    content,
    createdAt,
    className,
    isDeleteable = false,
    onDelete = undefined
}: CardReviewProps) {
    return (
        <div
            className={cn(
                "flex flex-col gap-4 min-w-50 sm:min-w-75 max-w-sm",
                "interactive-scale",
                className
            )}
        >
            <CardReview
                content={content}
                createdAt={createdAt}
                rating={rating}
                title={station}
                user={{
                    name,
                    avatar
                }}
                isDeleteable={isDeleteable}
                onDelete={onDelete}
            />
        </div>
    )
}
